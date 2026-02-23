namespace KSS.Dto
{
    public class CityDto
    {
        public int Id { get; set; }
        public short CountryId { get; set; }
        public short? RegionId { get; set; }
        public string? Code { get; set; }
    }

    public class CityTranslationDto
    {
        public int CityId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CityViewDto
    {
        public int Id { get; set; }
        public short CountryId { get; set; }
        public short? RegionId { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
