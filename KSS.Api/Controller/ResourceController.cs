using KSS.Dto;
using KSS.Entity;
using KSS.Service.IService;

namespace KSS.Api.Controller
{
    public class ResourceController : BaseController<Resource, ResourceDto, ResourceDto, ResourceDto>
    {
        public ResourceController(IResourceService service) : base(service) { }
    }
}
