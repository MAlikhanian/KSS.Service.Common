using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class ResourceRepository : BaseRepository<MainDbContext, Resource>, IResourceRepository
    {
        public ResourceRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
