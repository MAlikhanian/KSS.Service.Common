using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class RegionTranslationRepository : BaseRepository<MainDbContext, RegionTranslation>, IRegionTranslationRepository
    {
        public RegionTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
