using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class PhoneLabelRepository : BaseRepository<MainDbContext, PhoneLabel>, IPhoneLabelRepository
    {
        public PhoneLabelRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
