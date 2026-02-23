namespace KSS.Dto
{
    public class CountryDto
    {
        public short Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Code3 { get; set; } = string.Empty;
        public string? NativeName { get; set; }
        public short? CallingCode { get; set; }
    }

    public class CountryTranslationDto
    {
        public short CountryId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CountryViewDto
    {
        public short Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Code3 { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NativeName { get; set; }
        public short? CallingCode { get; set; }
    }
}
