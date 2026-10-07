using System.Net;
using System.Text.Json;
using KSS.Entity;
using Microsoft.EntityFrameworkCore;

namespace KSS.Api.Tests
{
    public class GetAcceptanceTests
    {
        [Fact] // T13
        public async Task T13_UnknownKey_Is404()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var response = await client.GetAcceptance(token, "unknownapp");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("TERMS_UNKNOWN_APPLICATION", await response.Message());

            // Control: a known key succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(token, "customerrisk")).StatusCode);
        }

        [Fact] // T14
        public async Task T14_MissingOrBlankKey_Is400()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            foreach (var key in new string?[] { null, "", "   " })
            {
                var response = await client.GetAcceptance(token, key);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("TERMS_BAD_REQUEST", await response.Message());
            }

            // Control: with the key present it succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(token, "customerrisk")).StatusCode);
        }

        [Fact] // T15
        public async Task T15_ConfiguredKeyWithNoModuleRow_Is404()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var response = await client.GetAcceptance(token, "nomodule");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("TERMS_UNKNOWN_APPLICATION", await response.Message());

            var post = await client.PostAccept(token, Http.AcceptBody("nomodule"));
            Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
            Assert.Equal(0, factory.CountAcceptances());
        }

        [Fact] // T16
        public async Task T16_InactiveOrDeletedModule_Is404()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            foreach (var key in new[] { "inactivemod", "deletedmod" })
            {
                var response = await client.GetAcceptance(token, key);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAccept(token, Http.AcceptBody(key))).StatusCode);
            }
            Assert.Equal(0, factory.CountAcceptances());

            // Control: the active module with the same configured version succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(token, "customerrisk")).StatusCode);
        }

        [Fact] // T17
        public async Task T17_NotAccepted_ReadsFalseWithNullTime()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();

            var body = await (await client.GetAcceptance(TestTokens.ForPerson(Guid.CreateVersion7()), "customerrisk")).Json();
            Assert.Equal("customerrisk", body.GetProperty("applicationKey").GetString());
            Assert.Equal(CommonTestFactory.CurrentVersion, body.GetProperty("version").GetString());
            Assert.False(body.GetProperty("accepted").GetBoolean());
            Assert.Equal(JsonValueKind.Null, body.GetProperty("acceptedAt").ValueKind);
        }

        [Fact] // T18
        public async Task T18_AcceptedCurrentVersion_ReadsTrueWithItsTime()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var accepted = await (await client.PostAccept(token, Http.AcceptBody())).Json();
            var read = await (await client.GetAcceptance(token, "customerrisk")).Json();

            Assert.True(read.GetProperty("accepted").GetBoolean());
            Assert.Equal(accepted.GetProperty("acceptedAt").GetString(), read.GetProperty("acceptedAt").GetString());
        }

        [Fact] // T19
        public async Task T19_AcceptedOnlyAnOlderVersion_ReadsFalse()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var person = Guid.CreateVersion7();

            using (var db = factory.NewContext())
            {
                db.TermsAcceptances.Add(new TermsAcceptance
                {
                    Id = Guid.CreateVersion7(), PersonId = person, ModuleId = CommonTestFactory.CustomerRiskModuleId,
                    TermsVersion = "v0", AcceptedAt = DateTime.UtcNow.AddDays(-30), CreatedBy = person
                });
                await db.SaveChangesAsync();
            }

            var read = await (await client.GetAcceptance(TestTokens.ForPerson(person), "customerrisk")).Json();
            Assert.False(read.GetProperty("accepted").GetBoolean());
            Assert.Equal(CommonTestFactory.CurrentVersion, read.GetProperty("version").GetString());
        }

        [Fact] // T20
        public async Task T20_KeyIsCaseInsensitiveAndTrimmed_AndTheResponseEchoesTheCanonicalCode()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            foreach (var spelling in new[] { "CustomerRisk", "  CUSTOMERRISK  " })
            {
                var response = await client.GetAcceptance(token, spelling);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("customerrisk", (await response.Json()).GetProperty("applicationKey").GetString());
            }
        }
    }

    public class PostAcceptTests
    {
        [Fact] // T21
        public async Task T21_AcceptCurrent_WritesOneVersion7Row_AndLeavesTheUnmappedAuditColumnsNull()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var person = Guid.CreateVersion7();

            var before = DateTime.UtcNow.AddSeconds(-1);
            var response = await client.PostAccept(TestTokens.ForPerson(person), Http.AcceptBody());
            var after = DateTime.UtcNow.AddSeconds(1);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Json();
            Assert.True(body.GetProperty("accepted").GetBoolean());
            var acceptedAt = DateTime.Parse(body.GetProperty("acceptedAt").GetString()!).ToUniversalTime();
            Assert.InRange(acceptedAt, before, after);

            Assert.Equal(1, factory.CountAcceptances());
            using var command = factory.Connection.CreateCommand();
            command.CommandText = "SELECT Id, PersonId, CreatedBy, UpdatedBy, UpdatedAt, DeletedBy, DeletedAt FROM TermsAcceptance";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(7, Guid.Parse(reader.GetString(0)).Version);
            Assert.Equal(person, Guid.Parse(reader.GetString(1)));
            Assert.Equal(person, Guid.Parse(reader.GetString(2)));     // CreatedBy == PersonId
            for (var column = 3; column <= 6; column++)
                Assert.True(reader.IsDBNull(column), $"column {reader.GetName(column)} must be NULL");

            // Why they stay NULL: nothing on the entity for the context to stamp.
            Assert.Null(typeof(TermsAcceptance).GetProperty("UpdatedAt"));
            Assert.Null(typeof(TermsAcceptance).GetProperty("UpdatedBy"));
        }

        [Fact] // T22
        public async Task T22_RepeatAccept_ReturnsTheOriginalTime_AndStillOneRow()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var first = await (await client.PostAccept(token, Http.AcceptBody())).Json();
            await Task.Delay(20);
            var second = await client.PostAccept(token, Http.AcceptBody());

            Assert.Equal(HttpStatusCode.OK, second.StatusCode);
            Assert.Equal(first.GetProperty("acceptedAt").GetString(), (await second.Json()).GetProperty("acceptedAt").GetString());
            Assert.Equal(1, factory.CountAcceptances());
        }

        [Fact] // T23
        public async Task T23_StaleVersion_Is409WithTheCurrentVersion_AndNothingIsWritten()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            foreach (var stale in new[] { "v0", "v2", "V1" })
            {
                var response = await client.PostAccept(token, Http.AcceptBody(version: stale));
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
                var body = await response.Json();
                Assert.Equal("TERMS_STALE", body.GetProperty("message").GetString());
                Assert.Equal(CommonTestFactory.CurrentVersion, body.GetProperty("currentVersion").GetString());
            }
            Assert.Equal(0, factory.CountAcceptances());

            // Control: the current version is accepted.
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(token, Http.AcceptBody())).StatusCode);
        }

        [Fact] // T24
        public async Task T24_MissingOrBlankVersionOrKey_OrMalformedJson_Is400()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var bodies = new[]
            {
                """{"applicationKey":"customerrisk"}""",
                """{"applicationKey":"customerrisk","version":""}""",
                """{"applicationKey":"customerrisk","version":"   "}""",
                """{"version":"v1"}""",
                """{"applicationKey":" ","version":"v1"}""",
                """{"applicationKey":"customerrisk","version":"v1""",   // malformed
                ""                                                      // empty body
            };
            foreach (var raw in bodies)
            {
                var response = await client.PostAccept(token, raw);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("TERMS_BAD_REQUEST", await response.Message());
            }
            Assert.Equal(0, factory.CountAcceptances());

            // Control: a complete body succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(token, Http.AcceptBody())).StatusCode);
        }

        [Fact] // T25
        public async Task T25_UnknownProperty_IncludingPersonId_Is400_AndNothingIsWritten()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            foreach (var raw in new[]
            {
                $$"""{"applicationKey":"customerrisk","version":"v1","personId":"{{Guid.CreateVersion7()}}"}""",
                """{"applicationKey":"customerrisk","version":"v1","extra":1}"""
            })
            {
                var response = await client.PostAccept(token, raw);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("TERMS_BAD_REQUEST", await response.Message());
            }
            Assert.Equal(0, factory.CountAcceptances());

            // Control: the same body without the extra property succeeds.
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(token, Http.AcceptBody())).StatusCode);
        }

        [Fact] // T26
        public async Task T26_ConcurrentAccept_ReturnsTheWinnersTime_AndOneRow()
        {
            CommonTestFactory? factory = null;
            var race = new CompetingInsertInterceptor(() => factory!.Connection);
            factory = new CommonTestFactory(null, race);
            using var _ = factory;
            var client = factory.CreateSeededClient();

            var response = await client.PostAccept(TestTokens.ForPerson(Guid.CreateVersion7()), Http.AcceptBody());

            Assert.True(race.Fired);                                   // the competing insert happened in the window
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var acceptedAt = DateTime.Parse((await response.Json()).GetProperty("acceptedAt").GetString()!).ToUniversalTime();
            Assert.Equal(race.WinnerAcceptedAt, acceptedAt);           // the first row's time, not this request's
            Assert.Equal(1, factory.CountAcceptances());
        }

        [Fact] // T27
        public async Task T27_NoCompanyClaim_GetAndPostSucceed()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            // Only personId (plus the usual userName and permission); no company claim of any kind.
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            Assert.Equal(HttpStatusCode.OK, (await client.GetAcceptance(token, "customerrisk")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(token, Http.AcceptBody())).StatusCode);
        }

        [Fact] // T28
        public async Task T28_OnePersonsAcceptanceIsNotAnothers()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();
            var a = Guid.CreateVersion7();
            var b = Guid.CreateVersion7();

            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(TestTokens.ForPerson(a), Http.AcceptBody())).StatusCode);

            var readB = await (await client.GetAcceptance(TestTokens.ForPerson(b), "customerrisk")).Json();
            Assert.False(readB.GetProperty("accepted").GetBoolean());

            Assert.Equal(HttpStatusCode.OK, (await client.PostAccept(TestTokens.ForPerson(b), Http.AcceptBody())).StatusCode);
            Assert.Equal(2, factory.CountAcceptances());
            using var db = factory.NewContext();
            Assert.Equal(new[] { a, b }.OrderBy(x => x), (await db.TermsAcceptances.Select(x => x.PersonId).ToListAsync()).OrderBy(x => x));
        }
    }
}
