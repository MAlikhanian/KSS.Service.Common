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

            // Address / Phone labels
            CreateMap<AddressLabel, AddressLabelDto>().ReverseMap();
            CreateMap<AddressLabelTranslation, AddressLabelTranslationDto>().ReverseMap();
            CreateMap<PhoneLabel, PhoneLabelDto>().ReverseMap();
            CreateMap<PhoneLabelTranslation, PhoneLabelTranslationDto>().ReverseMap();

            // Module
            CreateMap<Module, ModuleDto>().ReverseMap();
            CreateMap<ModuleTranslation, ModuleTranslationDto>().ReverseMap();

            // Resource
            CreateMap<Resource, ResourceDto>().ReverseMap();
            CreateMap<ResourceTranslation, ResourceTranslationDto>().ReverseMap();
        }
    }
}
