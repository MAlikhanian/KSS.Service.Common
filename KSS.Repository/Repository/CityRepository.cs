using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class CityRepository : BaseRepository<MainDbContext, City>, ICityRepository
    {
        public CityRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
