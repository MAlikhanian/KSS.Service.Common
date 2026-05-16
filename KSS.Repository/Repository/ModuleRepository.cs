using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class ModuleRepository : BaseRepository<MainDbContext, Module>, IModuleRepository
    {
        public ModuleRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
