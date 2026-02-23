using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class CityTranslationController : BaseController<CityTranslation, CityTranslationDto, CityTranslationDto, CityTranslationDto>
    {
        public CityTranslationController(ICityTranslationService service) : base(service) { }
    }
}
