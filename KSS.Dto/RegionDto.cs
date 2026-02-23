namespace KSS.Dto
{
    public class RegionDto
    {
        public short Id { get; set; }
        public short CountryId { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class RegionTranslationDto
    {
        public short RegionId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RegionViewDto
    {
        public short Id { get; set; }
        public short CountryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
