using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class ResourceService : BaseService<Resource, ResourceDto, ResourceDto, ResourceDto>, IResourceService
    {
        public ResourceService(IMapper mapper, IResourceRepository repository) : base(mapper, repository) { }
    }
}
