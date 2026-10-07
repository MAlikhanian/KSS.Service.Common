using System.Net;
using System.Reflection;
using System.Security.Claims;
using KSS.Api.Controller;
using KSS.Data.DbContexts;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KSS.Api.Tests
{
    public class WiringAndAuthTests
    {
        private static bool DerivesFromBaseController(Type type)
        {
            for (var t = type.BaseType; t is not null; t = t.BaseType)
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BaseController<,,,>))
                    return true;
            return false;
        }

        [Fact] // T7
        public void T07_TermsControllerIsWired_WithExactlyTwoActions_AndNotTheGenericBase()
        {
            using var factory = new CommonTestFactory();
            factory.CreateSeededClient();

            // Discovered as a controller by the running application.
            var feature = new ControllerFeature();
            factory.Services.GetRequiredService<ApplicationPartManager>().PopulateFeature(feature);
            Assert.Contains(typeof(TermsController).GetTypeInfo(), feature.Controllers);

            // Exactly two actions, on exactly these routes and verbs.
            var actions = factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>()
                .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                .Where(a => a.ControllerTypeInfo.AsType() == typeof(TermsController))
                .Select(a => (Route: a.AttributeRouteInfo!.Template!,
                              Verb: a.ActionConstraints!.OfType<HttpMethodActionConstraint>().Single().HttpMethods.Single()))
                .OrderBy(a => a.Route)
                .ToList();
            Assert.Equal(new[] { ("Api/Terms/Accept", "POST"), ("Api/Terms/MyAcceptance", "GET") }, actions);

            // Not the generic base that exposes add/update/remove.
            Assert.False(DerivesFromBaseController(typeof(TermsController)));
            // Control: the check fires on a controller that does derive from it.
            Assert.True(DerivesFromBaseController(typeof(ModuleController)));
        }

        [Fact] // T8
        public async Task T08_NoToken_Is401_OnBothEndpoints()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();

            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAcceptance(null, "customerrisk")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAccept(null, Http.AcceptBody())).StatusCode);
            Assert.Equal(0, factory.CountAcceptances());

            // Control: the same requests with a valid token succeed.
            var token = TestTokens.ForPerson(Guid.CreateVersion7());
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(token, "customerrisk")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(token, Http.AcceptBody())).StatusCode);
        }

        [Fact] // T9
        public async Task T09_TokenSignedWithAnotherKey_Is401()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var person = Guid.CreateVersion7();
            var foreign = TestTokens.Mint(new[] { new Claim("personId", person.ToString()) }, TestTokens.ForeignSigningKey);

            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAcceptance(foreign, "customerrisk")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAccept(foreign, Http.AcceptBody())).StatusCode);
            Assert.Equal(0, factory.CountAcceptances());

            // Control: the same claims signed with the right key succeed.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(TestTokens.ForPerson(person), "customerrisk")).StatusCode);
        }

        [Fact] // T10
        public async Task T10_ExpiredToken_Is401()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var person = Guid.CreateVersion7();
            // Expired well beyond the default clock skew.
            var expired = TestTokens.Mint(new[] { new Claim("personId", person.ToString()) }, expires: DateTime.UtcNow.AddMinutes(-30));

            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAcceptance(expired, "customerrisk")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAccept(expired, Http.AcceptBody())).StatusCode);

            // Control: an unexpired token for the same person succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(TestTokens.ForPerson(person), "customerrisk")).StatusCode);
        }

        [Fact] // T11
        public async Task T11_MissingEmptySystemOrNonGuidPersonId_Is403_AndNothingIsWritten()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();

            var tokens = new[]
            {
                TestTokens.Mint(Array.Empty<Claim>()),                                       // no personId claim
                TestTokens.Mint(new[] { new Claim("personId", Guid.Empty.ToString()) }),     // empty id
                TestTokens.Mint(new[] { new Claim("personId", CommonTestFactory.SystemId.ToString()) }), // system id
                TestTokens.Mint(new[] { new Claim("personId", "not-a-guid") })               // not a GUID
            };
            foreach (var token in tokens)
            {
                var get = await client.GetAcceptance(token, "customerrisk");
                Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
                Assert.Equal("TERMS_NO_PERSON", await get.Message());

                var post = await client.PostAccept(token, Http.AcceptBody());
                Assert.Equal(HttpStatusCode.Forbidden, post.StatusCode);
                Assert.Equal("TERMS_NO_PERSON", await post.Message());
            }
            Assert.Equal(0, factory.CountAcceptances());

            // Control: a real person id is accepted and writes one row.
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(TestTokens.ForPerson(Guid.CreateVersion7()), Http.AcceptBody())).StatusCode);
            Assert.Equal(1, factory.CountAcceptances());
        }

        [Fact] // T12
        public async Task T12_OnlyThePersonIdClaimReachesTheService_UnderCommonsInboundClaimMapping()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var person = Guid.CreateVersion7();

            // A token whose only id claim is personId produces a row for exactly that person,
            // through the real authentication pipeline with Common's default inbound claim mapping.
            var ok = await client.PostAccept(TestTokens.ForPerson(person), Http.AcceptBody());
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
            using (var db = factory.NewContext())
            {
                var row = await db.TermsAcceptances.SingleAsync();
                Assert.Equal(person, row.PersonId);
            }

            // The same id carried as 'sub' instead does not reach the service: 403, nothing written.
            var subOnly = TestTokens.Mint(new[] { new Claim("sub", Guid.CreateVersion7().ToString()) });
            var refused = await client.PostAccept(subOnly, Http.AcceptBody());
            Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
            Assert.Equal(1, factory.CountAcceptances());
        }
    }
}
