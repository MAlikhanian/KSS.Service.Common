using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class ModuleTranslationService : BaseService<ModuleTranslation, ModuleTranslationDto, ModuleTranslationDto, ModuleTranslationDto>, IModuleTranslationService
    {
        public ModuleTranslationService(IMapper mapper, IModuleTranslationRepository repository) : base(mapper, repository) { }
    }
}
