using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class CountryController : BaseController<Country, CountryDto, CountryDto, CountryDto>
    {
        public CountryController(ICountryService service) : base(service) { }
    }
}
