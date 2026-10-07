using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using KSS.Data.DbContexts;
using KSS.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace KSS.Api.Tests
{
    /// <summary>Mints HS256 tokens shaped like the ones the Auth service issues.</summary>
    public static class TestTokens
    {
        // Test-only signing keys. They sign tokens inside this test run and nowhere else.
        public const string SigningKey = "test-only-signing-key-0000000000000000000000000000000000000000000";
        public const string ForeignSigningKey = "foreign-test-key-1111111111111111111111111111111111111111111111";

        public static string ForPerson(Guid personId) => Mint(new[] { new Claim("personId", personId.ToString()) });

        public static string Mint(IEnumerable<Claim> claims, string? key = null, DateTime? expires = null)
        {
            var all = new List<Claim>(claims) { new("userName", "test-user"), new("permission", "none") };
            var exp = expires ?? DateTime.UtcNow.AddMinutes(30);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key ?? SigningKey)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(claims: all, notBefore: exp.AddHours(-1), expires: exp, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// Hosts Common in memory. Configuration is replaced entirely (no local settings file is ever read),
    /// and the database is SQLite in-memory, which enforces the unique key and the foreign key.
    /// No real database is touched.
    /// </summary>
    public sealed class CommonTestFactory : WebApplicationFactory<Program>
    {
        public const string TestConnectionString = "Server=test.invalid;Database=KSS_Common_Dev;Encrypt=True";
        public const string CurrentVersion = "v1";

        public static readonly Guid CustomerRiskModuleId = Guid.Parse("019F1000-0000-7001-8000-000000000011");
        public static readonly Guid InactiveModuleId = Guid.Parse("019F1000-0000-7001-8000-0000000000F1");
        public static readonly Guid DeletedModuleId = Guid.Parse("019F1000-0000-7001-8000-0000000000F2");
        public static readonly Guid SystemId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly Dictionary<string, string?> _settings;
        private readonly IInterceptor[] _interceptors;

        public SqliteConnection Connection { get; } = new("DataSource=:memory:");

        public CommonTestFactory(Dictionary<string, string?>? settings = null, params IInterceptor[] interceptors)
        {
            _settings = settings ?? DefaultSettings();
            _interceptors = interceptors;
            Connection.Open();
        }

        public static Dictionary<string, string?> DefaultSettings() => new()
        {
            ["JWT_SECRET"] = TestTokens.SigningKey,
            ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
            ["Terms:CurrentVersion:customerrisk"] = CurrentVersion,
            ["Terms:CurrentVersion:inactivemod"] = CurrentVersion,
            ["Terms:CurrentVersion:deletedmod"] = CurrentVersion,
            ["Terms:CurrentVersion:nomodule"] = CurrentVersion   // configured, but no Module row
        };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.Sources.Clear();                 // never read a local settings file
                config.AddInMemoryCollection(_settings);
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<MainDbContext>>();
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<MainDbContext>));
                services.AddDbContext<MainDbContext>(options =>
                {
                    options.UseSqlite(Connection);
                    if (_interceptors.Length > 0)
                        options.AddInterceptors(_interceptors);
                });
            });
        }

        /// <summary>Starts the host, creates the schema and seeds the Module rows.</summary>
        public HttpClient CreateSeededClient()
        {
            var client = CreateClient();
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            db.Database.EnsureCreated();

            // The real table also has these columns; the entity deliberately does not map them.
            foreach (var column in new[] { "UpdatedBy TEXT NULL", "UpdatedAt TEXT NULL", "DeletedBy TEXT NULL", "DeletedAt TEXT NULL" })
                Execute($"ALTER TABLE TermsAcceptance ADD COLUMN {column}");

            db.Modules.AddRange(
                new Module { Id = CustomerRiskModuleId, Code = "customerrisk", CreatedBy = SystemId, IsActive = true },
                new Module { Id = InactiveModuleId, Code = "inactivemod", CreatedBy = SystemId, IsActive = false },
                new Module { Id = DeletedModuleId, Code = "deletedmod", CreatedBy = SystemId, IsActive = true, DeletedAt = DateTime.UtcNow, DeletedBy = SystemId });
            db.SaveChanges();
            return client;
        }

        public void Execute(string sql)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public long CountAcceptances()
        {
            using var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM TermsAcceptance";
            return (long)command.ExecuteScalar()!;
        }

        /// <summary>A context on the same in-memory database, outside the host (for arranging rows).</summary>
        public MainDbContext NewContext() =>
            new(new DbContextOptionsBuilder<MainDbContext>().UseSqlite(Connection).Options);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                Connection.Dispose();
        }
    }

    public static class Http
    {
        public static Task<HttpResponseMessage> GetAcceptance(this HttpClient client, string? token, string? applicationKey)
        {
            var url = applicationKey is null
                ? "/Api/Terms/MyAcceptance"
                : "/Api/Terms/MyAcceptance?applicationKey=" + Uri.EscapeDataString(applicationKey);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PostAccept(this HttpClient client, string? token, string rawJson)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/Api/Terms/Accept")
            {
                Content = new StringContent(rawJson, Encoding.UTF8, "application/json")
            };
            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client.SendAsync(request);
        }

        public static string AcceptBody(string applicationKey = "customerrisk", string version = CommonTestFactory.CurrentVersion) =>
            JsonSerializer.Serialize(new { applicationKey, version });

        public static async Task<JsonElement> Json(this HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }

        public static async Task<string?> Message(this HttpResponseMessage response) =>
            (await response.Json()).GetProperty("message").GetString();
    }

    /// <summary>
    /// Inserts a competing acceptance for the same (person, module, version) just before the service's own
    /// SaveChanges runs: the exact window between the existence check and the insert.
    /// </summary>
    public sealed class CompetingInsertInterceptor : SaveChangesInterceptor
    {
        private readonly Func<SqliteConnection> _connection;
        public DateTime WinnerAcceptedAt { get; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public bool Fired { get; private set; }

        public CompetingInsertInterceptor(Func<SqliteConnection> connection) => _connection = connection;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var pending = eventData.Context?.ChangeTracker.Entries<TermsAcceptance>()
                .FirstOrDefault(e => e.State == EntityState.Added)?.Entity;
            if (pending is not null && !Fired)
            {
                Fired = true;
                using var other = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>().UseSqlite(_connection()).Options);
                other.TermsAcceptances.Add(new TermsAcceptance
                {
                    Id = Guid.CreateVersion7(),
                    PersonId = pending.PersonId,
                    ModuleId = pending.ModuleId,
                    TermsVersion = pending.TermsVersion,
                    AcceptedAt = WinnerAcceptedAt,
                    CreatedBy = pending.PersonId
                });
                other.SaveChanges();
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    public static class Exceptions
    {
        public static T? Find<T>(Exception? exception) where T : Exception
        {
            for (var e = exception; e is not null; e = e.InnerException)
            {
                if (e is T match)
                    return match;
                if (e is AggregateException aggregate)
                    foreach (var inner in aggregate.InnerExceptions)
                        if (Find<T>(inner) is { } found)
                            return found;
            }
            return null;
        }
    }
}
