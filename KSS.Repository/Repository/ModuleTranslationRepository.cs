using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class ModuleTranslationRepository : BaseRepository<MainDbContext, ModuleTranslation>, IModuleTranslationRepository
    {
        public ModuleTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
