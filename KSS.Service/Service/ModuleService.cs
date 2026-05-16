using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class ModuleService : BaseService<Module, ModuleDto, ModuleDto, ModuleDto>, IModuleService
    {
        public ModuleService(IMapper mapper, IModuleRepository repository) : base(mapper, repository) { }
    }
}
