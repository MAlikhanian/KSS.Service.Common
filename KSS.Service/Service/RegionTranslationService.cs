using AutoMapper;
using KSS.Dto;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Service.IService;

namespace KSS.Service.Service
{
    public class RegionTranslationService : BaseService<RegionTranslation, RegionTranslationDto, RegionTranslationDto, RegionTranslationDto>, IRegionTranslationService
    {
        public RegionTranslationService(IMapper mapper, IRegionTranslationRepository repository) : base(mapper, repository) { }
    }
}
