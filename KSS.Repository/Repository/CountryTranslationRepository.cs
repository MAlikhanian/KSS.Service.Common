using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class CountryTranslationRepository : BaseRepository<MainDbContext, CountryTranslation>, ICountryTranslationRepository
    {
        public CountryTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
