using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class AddressLabelTranslationRepository : BaseRepository<MainDbContext, AddressLabelTranslation>, IAddressLabelTranslationRepository
    {
        public AddressLabelTranslationRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
