using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class LanguageService : BaseService<Language, LanguageDto, LanguageDto, LanguageDto>, ILanguageService
    {
        public LanguageService(IMapper mapper, ILanguageRepository repository) : base(mapper, repository) { }
    }
}
