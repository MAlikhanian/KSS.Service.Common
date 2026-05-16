using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class ResourceTranslationRepository : BaseRepository<MainDbContext, ResourceTranslation>, IResourceTranslationRepository
    {
        public ResourceTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
