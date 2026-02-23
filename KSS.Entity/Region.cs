using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Region")]
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; }

        public short CountryId { get; set; }

        [Required, MaxLength(3), Column(TypeName = "varchar(3)")]
        public string Code { get; set; } = string.Empty;

        [ForeignKey("CountryId")]
        public Country Country { get; set; } = null!;

        public ICollection<RegionTranslation> Translations { get; set; } = new List<RegionTranslation>();
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
