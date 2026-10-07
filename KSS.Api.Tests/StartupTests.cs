using System.Net;
using KSS.Api.ServiceExtention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KSS.Api.Tests
{
    /// <summary>Startup and configuration: an invalid entry must stop the host, and say which key without the value.</summary>
    public class StartupTests
    {
        private static InvalidOperationException StartupFailure(Dictionary<string, string?> settings)
        {
            using var factory = new CommonTestFactory(settings);
            var thrown = Record.Exception(() => factory.CreateClient());
            var failure = Exceptions.Find<InvalidOperationException>(thrown);
            Assert.NotNull(failure);
            return failure!;
        }

        private static Dictionary<string, string?> With(string key, string? value)
        {
            var settings = CommonTestFactory.DefaultSettings();
            settings[key] = value;
            return settings;
        }

        [Fact] // T1
        public async Task T01_ValidConfigStarts_AndNoLocalSettingsFileIsRead()
        {
            using var factory = new CommonTestFactory();
            var client = factory.CreateSeededClient();

            var configuration = factory.Services.GetRequiredService<IConfiguration>();
            // The host sees only the test configuration: nothing from a local settings file.
            Assert.Null(configuration["TokenSetting:JwtSecret"]);
            Assert.Equal(CommonTestFactory.TestConnectionString, configuration["ConnectionStrings:DefaultConnection"]);

            var response = await client.GetAcceptance(TestTokens.ForPerson(Guid.CreateVersion7()), "customerrisk");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact] // T2
        public void T02_BlankVersionStopsTheHost()
        {
            var failure = StartupFailure(With("Terms:CurrentVersion:customerrisk", ""));
            Assert.Contains("Terms:CurrentVersion:customerrisk", failure.Message);
        }

        [Fact] // T3
        public void T03_ThirtyThreeCharacterVersionStopsTheHost()
        {
            var tooLong = "VERSIONMARK" + new string('x', 22); // 33 characters
            Assert.Equal(33, tooLong.Length);
            var failure = StartupFailure(With("Terms:CurrentVersion:customerrisk", tooLong));
            Assert.Contains("Terms:CurrentVersion:customerrisk", failure.Message);
            Assert.DoesNotContain("VERSIONMARK", failure.Message);

            // Control: 32 characters is accepted.
            using var factory = new CommonTestFactory(With("Terms:CurrentVersion:customerrisk", new string('x', 32)));
            factory.CreateSeededClient();
        }

        [Fact] // T4
        public void T04_SurroundingSpaceOrInnerTabCrLfStopsTheHost()
        {
            foreach (var bad in new[] { " VERSIONMARK", "VERSIONMARK ", "VERSION\tMARK", "VERSION\rMARK", "VERSION\nMARK" })
            {
                var failure = StartupFailure(With("Terms:CurrentVersion:customerrisk", bad));
                Assert.Contains("Terms:CurrentVersion:customerrisk", failure.Message);
                Assert.DoesNotContain("MARK", failure.Message);
            }

            // Control: an inner space is allowed.
            using var factory = new CommonTestFactory(With("Terms:CurrentVersion:customerrisk", "2026 10 01"));
            factory.CreateSeededClient();
        }

        [Fact] // T5
        public void T05_UppercaseHyphenOrOverlongKeyStopsTheHost()
        {
            foreach (var badKey in new[] { "CustomerRisk", "customer-risk", new string('k', 31) })
            {
                // Configuration keys are case-insensitive: next to the valid lowercase entry an uppercase key
                // would merge into it. Remove it so each bad key is the only spelling present.
                var settings = CommonTestFactory.DefaultSettings();
                settings.Remove("Terms:CurrentVersion:customerrisk");
                settings["Terms:CurrentVersion:" + badKey] = "v1";
                var failure = StartupFailure(settings);
                Assert.Contains("Terms:CurrentVersion:", failure.Message);
            }

            // Control: a 30-character lowercase key is accepted.
            using var factory = new CommonTestFactory(With("Terms:CurrentVersion:" + new string('k', 30), "v1"));
            factory.CreateSeededClient();
        }

        [Fact] // T6
        public async Task T06_NoTermsSectionStarts_AndEveryKeyIs404()
        {
            var settings = CommonTestFactory.DefaultSettings();
            foreach (var key in settings.Keys.Where(k => k.StartsWith("Terms:")).ToList())
                settings.Remove(key);

            using var factory = new CommonTestFactory(settings);
            var client = factory.CreateSeededClient();
            var token = TestTokens.ForPerson(Guid.CreateVersion7());

            var get = await client.GetAcceptance(token, "customerrisk");     // the Module row exists, no version
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
            Assert.Equal("TERMS_UNKNOWN_APPLICATION", await get.Message());

            var post = await client.PostAccept(token, Http.AcceptBody());
            Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
            Assert.Equal(0, factory.CountAcceptances());
        }

        [Fact] // step 0, test 1
        public void S1_BlankConnectionStringStopsTheHost_NamingTheKeyOnly()
        {
            foreach (var blank in new string?[] { null, "", "   " })
            {
                var failure = StartupFailure(With("ConnectionStrings:DefaultConnection", blank));
                Assert.Contains(CommonServiceExtention.ConnectionStringKey, failure.Message);
            }
        }

        [Fact] // step 0, test 2
        public void S2_NonCommonOrMalformedConnectionStringStopsTheHost_WithoutEchoingTheValue()
        {
            var other = StartupFailure(With("ConnectionStrings:DefaultConnection",
                "Server=test.invalid;Database=OTHERDBMARK;Encrypt=True"));
            Assert.Contains(CommonServiceExtention.ConnectionStringKey, other.Message);
            Assert.DoesNotContain("OTHERDBMARK", other.Message);

            var malformed = StartupFailure(With("ConnectionStrings:DefaultConnection", "MALFORMEDMARK-no-key-value-pairs"));
            Assert.Contains(CommonServiceExtention.ConnectionStringKey, malformed.Message);
            Assert.DoesNotContain("MALFORMEDMARK", malformed.Message);

            // Control: both Common databases are accepted, case-insensitively.
            foreach (var database in new[] { "KSS_Common_Dev", "kss_common_prod" })
            {
                using var factory = new CommonTestFactory(With("ConnectionStrings:DefaultConnection",
                    $"Server=test.invalid;Database={database};Encrypt=True"));
                factory.CreateSeededClient();
            }
        }
    }
}
