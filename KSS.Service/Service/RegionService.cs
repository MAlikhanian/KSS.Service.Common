using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class RegionService : BaseService<Region, RegionDto, RegionDto, RegionDto>, IRegionService
    {
        public RegionService(IMapper mapper, IRegionRepository repository) : base(mapper, repository) { }
    }
}
