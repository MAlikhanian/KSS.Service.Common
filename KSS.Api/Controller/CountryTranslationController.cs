using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class CountryTranslationController : BaseController<CountryTranslation, CountryTranslationDto, CountryTranslationDto, CountryTranslationDto>
    {
        public CountryTranslationController(ICountryTranslationService service) : base(service) { }
    }
}
