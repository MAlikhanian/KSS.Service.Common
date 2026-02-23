using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.EntityFrameworkCore;
using KSS.Data.DbContexts;

namespace KSS.Api.ServiceExtention
{
    public static class CommonServiceExtention
    {
        public static IServiceCollection AddCommonServiceExtention(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"];

            services.AddDbContext<MainDbContext>(options => options.UseSqlServer(connectionString));

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

            return services;
        }
    }
}
