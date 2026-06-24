namespace KSS.Dto
{
    public class AddressLabelDto
    {
        public byte Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class AddressLabelTranslationDto
    {
        public byte AddressLabelId { get; set; }
        public short LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
