using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class ModuleController : BaseController<Module, ModuleDto, ModuleDto, ModuleDto>
    {
        public ModuleController(IModuleService service) : base(service) { }
    }
}
