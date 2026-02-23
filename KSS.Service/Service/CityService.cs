using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CityService : BaseService<City, CityDto, CityDto, CityDto>, ICityService
    {
        public CityService(IMapper mapper, ICityRepository repository) : base(mapper, repository) { }
    }
}
