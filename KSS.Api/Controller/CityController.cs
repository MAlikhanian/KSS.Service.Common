using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class CityController : BaseController<City, CityDto, CityDto, CityDto>
    {
        public CityController(ICityService service) : base(service) { }
    }
}
