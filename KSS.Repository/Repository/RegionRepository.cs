using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class RegionRepository : BaseRepository<MainDbContext, Region>, IRegionRepository
    {
        public RegionRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
