using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class ModuleTranslationController : BaseController<ModuleTranslation, ModuleTranslationDto, ModuleTranslationDto, ModuleTranslationDto>
    {
        public ModuleTranslationController(IModuleTranslationService service) : base(service) { }
    }
}
