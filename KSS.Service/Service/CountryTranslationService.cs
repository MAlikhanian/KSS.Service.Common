using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CountryTranslationService : BaseService<CountryTranslation, CountryTranslationDto, CountryTranslationDto, CountryTranslationDto>, ICountryTranslationService
    {
        public CountryTranslationService(IMapper mapper, ICountryTranslationRepository repository) : base(mapper, repository) { }
    }
}
