using Microsoft.EntityFrameworkCore;
using KSS.Entity;

namespace KSS.Data.DbContexts
{
    public partial class MainDbContext
    {
        // Core lookup tables
        public DbSet<Language> Languages { get; set; }

        // Geography
        public DbSet<Country> Countries { get; set; }
        public DbSet<CountryTranslation> CountryTranslations { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegionTranslation> RegionTranslations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<CityTranslation> CityTranslations { get; set; }
    }
}
