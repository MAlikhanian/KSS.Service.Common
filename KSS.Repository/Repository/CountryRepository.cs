using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class CountryRepository : BaseRepository<MainDbContext, Country>, ICountryRepository
    {
        public CountryRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
