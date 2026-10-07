using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using KSS.Service.Terms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using KSS.Data.DbContexts;

namespace KSS.Api.ServiceExtention
{
    public static class CommonServiceExtention
    {
        public const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";

        private static readonly string[] CommonDatabases = { "KSS_Common_Dev", "KSS_Common_Prod" };

        public static IServiceCollection AddCommonServiceExtention(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            // Fail at startup, not on the first query: the connection string comes from the environment
            // (a Kubernetes Secret), and a missing or wrong one must stop the host loudly.
            RequireCommonConnectionString(connectionString);

            services.AddDbContext<MainDbContext>(options => options.UseSqlServer(connectionString));

            // Terms acceptance. The version map is validated here, so an invalid entry stops the host.
            services.AddSingleton(TermsOptions.FromEntries(
                configuration.GetSection(TermsOptions.SectionPath).GetChildren()
                    .Select(entry => new KeyValuePair<string, string?>(entry.Key, entry.Value))));
            services.TryAddSingleton(TimeProvider.System);
            services.AddScoped<TermsService>();

            // Language
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ILanguageService, LanguageService>();

            // Country
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICountryTranslationRepository, CountryTranslationRepository>();
            services.AddScoped<ICountryTranslationService, CountryTranslationService>();

            // Region
            services.AddScoped<IRegionRepository, RegionRepository>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<IRegionTranslationRepository, RegionTranslationRepository>();
            services.AddScoped<IRegionTranslationService, RegionTranslationService>();

            // City
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICityTranslationRepository, CityTranslationRepository>();
            services.AddScoped<ICityTranslationService, CityTranslationService>();

            // Address / Phone labels (shared lookups)
            services.AddScoped<IAddressLabelRepository, AddressLabelRepository>();
            services.AddScoped<IAddressLabelService, AddressLabelService>();
            services.AddScoped<IAddressLabelTranslationRepository, AddressLabelTranslationRepository>();
            services.AddScoped<IAddressLabelTranslationService, AddressLabelTranslationService>();
            services.AddScoped<IPhoneLabelRepository, PhoneLabelRepository>();
            services.AddScoped<IPhoneLabelService, PhoneLabelService>();
            services.AddScoped<IPhoneLabelTranslationRepository, PhoneLabelTranslationRepository>();
            services.AddScoped<IPhoneLabelTranslationService, PhoneLabelTranslationService>();

            // Module
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleTranslationRepository, ModuleTranslationRepository>();
            services.AddScoped<IModuleTranslationService, ModuleTranslationService>();

            // Resource
            services.AddScoped<IResourceRepository, ResourceRepository>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IResourceTranslationRepository, ResourceTranslationRepository>();
            services.AddScoped<IResourceTranslationService, ResourceTranslationService>();

            return services;
        }

        /// <summary>
        /// Throws unless the connection string is present and names one of Common's own databases.
        /// Messages name the configuration key, never the value. The parser's own exception is not
        /// surfaced either, because its message can echo the input.
        /// </summary>
        public static void RequireCommonConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"Configuration '{ConnectionStringKey}' is missing or empty. Provide it from the environment (ConnectionStrings__DefaultConnection).");

            string database;
            try
            {
                database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    $"Configuration '{ConnectionStringKey}' is not a valid SQL Server connection string.");
            }

            if (!CommonDatabases.Contains(database, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Configuration '{ConnectionStringKey}' must name the database KSS_Common_Dev or KSS_Common_Prod.");
        }
    }
}
