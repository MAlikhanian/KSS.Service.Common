using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class CityTranslationRepository : BaseRepository<MainDbContext, CityTranslation>, ICityTranslationRepository
    {
        public CityTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
