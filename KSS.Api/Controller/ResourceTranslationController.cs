using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class ResourceTranslationController : BaseController<ResourceTranslation, ResourceTranslationDto, ResourceTranslationDto, ResourceTranslationDto>
    {
        public ResourceTranslationController(IResourceTranslationService service) : base(service) { }
    }
}
