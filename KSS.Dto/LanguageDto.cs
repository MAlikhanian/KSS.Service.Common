namespace KSS.Dto
{
    public class LanguageDto
    {
        public short Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NativeName { get; set; }
        public bool IsActive { get; set; }
    }
}
