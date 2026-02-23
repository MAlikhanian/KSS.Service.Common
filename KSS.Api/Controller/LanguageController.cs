using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class LanguageController : BaseController<Language, LanguageDto, LanguageDto, LanguageDto>
    {
        public LanguageController(ILanguageService service) : base(service) { }
    }
}
