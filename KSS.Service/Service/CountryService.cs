using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class CountryService : BaseService<Country, CountryDto, CountryDto, CountryDto>, ICountryService
    {
        public CountryService(IMapper mapper, ICountryRepository repository) : base(mapper, repository) { }
    }
}
