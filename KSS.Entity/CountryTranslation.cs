using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CountryTranslation")]
    public class CountryTranslation
    {
        public short CountryId { get; set; }
        public short LanguageId { get; set; }

        [Required, MaxLength(80), Column(TypeName = "nvarchar(80)")]
        public string Name { get; set; } = string.Empty;

        [ForeignKey("CountryId")]
        public Country Country { get; set; } = null!;
    }
}
