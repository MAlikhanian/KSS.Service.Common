using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("RegionTranslation")]
    public class RegionTranslation
    {
        public short RegionId { get; set; }
        public short LanguageId { get; set; }

        [Required, MaxLength(80), Column(TypeName = "nvarchar(80)")]
        public string Name { get; set; } = string.Empty;

        [ForeignKey("RegionId")]
        public Region Region { get; set; } = null!;
    }
}
