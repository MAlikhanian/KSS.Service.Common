using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;

namespace KSS.Repository.Repository
{
    public class AddressLabelRepository : BaseRepository<MainDbContext, AddressLabel>, IAddressLabelRepository
    {
        public AddressLabelRepository(MainDbContext dbContext) : base(dbContext) { }
    }
}
