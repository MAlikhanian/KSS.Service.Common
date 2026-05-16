using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class ResourceTranslationService : BaseService<ResourceTranslation, ResourceTranslationDto, ResourceTranslationDto, ResourceTranslationDto>, IResourceTranslationService
    {
        public ResourceTranslationService(IMapper mapper, IResourceTranslationRepository repository) : base(mapper, repository) { }
    }
}
