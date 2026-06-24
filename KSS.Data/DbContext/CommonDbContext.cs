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

        // Shared address / phone labels
        public DbSet<AddressLabel> AddressLabels { get; set; }
        public DbSet<AddressLabelTranslation> AddressLabelTranslations { get; set; }
        public DbSet<PhoneLabel> PhoneLabels { get; set; }
        public DbSet<PhoneLabelTranslation> PhoneLabelTranslations { get; set; }

        // Authorization (Module / Resource)
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleTranslation> ModuleTranslations { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceTranslation> ResourceTranslations { get; set; }
    }
}
