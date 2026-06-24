namespace KSS.Dto
{
    public class PhoneLabelDto
    {
        public byte Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class PhoneLabelTranslationDto
    {
        public byte PhoneLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
