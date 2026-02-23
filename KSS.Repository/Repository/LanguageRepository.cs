using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class LanguageRepository : BaseRepository<MainDbContext, Language>, ILanguageRepository
    {
        public LanguageRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
