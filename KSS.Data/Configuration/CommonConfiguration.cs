using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KSS.Entity;

namespace KSS.Data.Configuration
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.Country).HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Regions).WithOne(x => x.Country).HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CountryTranslationConfiguration : IEntityTypeConfiguration<CountryTranslation>
    {
        public void Configure(EntityTypeBuilder<CountryTranslation> b)
        {
            b.HasKey(x => new { x.CountryId, x.LanguageId });
        }
    }

    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.Region).HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Cities).WithOne(x => x.Region).HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RegionTranslationConfiguration : IEntityTypeConfiguration<RegionTranslation>
    {
        public void Configure(EntityTypeBuilder<RegionTranslation> b)
        {
            b.HasKey(x => new { x.RegionId, x.LanguageId });
        }
    }

    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.City).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CityTranslationConfiguration : IEntityTypeConfiguration<CityTranslation>
    {
        public void Configure(EntityTypeBuilder<CityTranslation> b)
        {
            b.HasKey(x => new { x.CityId, x.LanguageId });
        }
    }

    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.Module).HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Resources).WithOne(x => x.Module).HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ModuleTranslationConfiguration : IEntityTypeConfiguration<ModuleTranslation>
    {
        public void Configure(EntityTypeBuilder<ModuleTranslation> b)
        {
            b.HasKey(x => new { x.ModuleId, x.LanguageId });
        }
    }

    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> b)
        {
            b.HasMany(x => x.Translations).WithOne(x => x.Resource).HasForeignKey(x => x.ResourceId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ResourceTranslationConfiguration : IEntityTypeConfiguration<ResourceTranslation>
    {
        public void Configure(EntityTypeBuilder<ResourceTranslation> b)
        {
            b.HasKey(x => new { x.ResourceId, x.LanguageId });
        }
    }
}
