using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CityTranslationService : BaseService<CityTranslation, CityTranslationDto, CityTranslationDto, CityTranslationDto>, ICityTranslationService
    {
        public CityTranslationService(IMapper mapper, ICityTranslationRepository repository) : base(mapper, repository) { }
    }
}
