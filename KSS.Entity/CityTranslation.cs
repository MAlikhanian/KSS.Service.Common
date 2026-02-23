using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CityTranslation")]
    public class CityTranslation
    {
        public int CityId { get; set; }
        public short LanguageId { get; set; }

        [Required, MaxLength(64), Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; } = string.Empty;

        [ForeignKey("CityId")]
        public City City { get; set; } = null!;
    }
}
