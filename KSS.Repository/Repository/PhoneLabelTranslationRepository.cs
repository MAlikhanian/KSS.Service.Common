using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class PhoneLabelTranslationRepository : BaseRepository<MainDbContext, PhoneLabelTranslation>, IPhoneLabelTranslationRepository
    {
        public PhoneLabelTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
