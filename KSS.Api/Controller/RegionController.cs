using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class RegionController : BaseController<Region, RegionDto, RegionDto, RegionDto>
    {
        public RegionController(IRegionService service) : base(service) { }
    }
}
