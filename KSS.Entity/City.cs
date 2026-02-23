using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("City")]
    public class City
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public short CountryId { get; set; }
        public short? RegionId { get; set; }

        [MaxLength(10), Column(TypeName = "varchar(10)")]
        public string? Code { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; } = null!;

        [ForeignKey("RegionId")]
        public Region? Region { get; set; }

        public ICollection<CityTranslation> Translations { get; set; } = new List<CityTranslation>();
    }
}
