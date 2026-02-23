using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class RegionTranslationController : BaseController<RegionTranslation, RegionTranslationDto, RegionTranslationDto, RegionTranslationDto>
    {
        public RegionTranslationController(IRegionTranslationService service) : base(service) { }
    }
}
