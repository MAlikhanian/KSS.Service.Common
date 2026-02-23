using AutoMapper;
using KSS.Entity;
using KSS.Dto;

namespace KSS.Api.MappingProfile
{
    public class BaseMappingProfile : Profile
    {
        public BaseMappingProfile()
        {
            // Language
            CreateMap<Language, LanguageDto>().ReverseMap();

            // Country
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<CountryTranslation, CountryTranslationDto>().ReverseMap();

            // Region
            CreateMap<Region, RegionDto>().ReverseMap();
            CreateMap<RegionTranslation, RegionTranslationDto>().ReverseMap();

            // City
            CreateMap<City, CityDto>().ReverseMap();
            CreateMap<CityTranslation, CityTranslationDto>().ReverseMap();
        }
    }
}
