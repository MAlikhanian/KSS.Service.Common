using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Country")]
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; }

        [Required, MaxLength(2), Column(TypeName = "nvarchar(2)")]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(3), Column(TypeName = "nvarchar(3)")]
        public string Code3 { get; set; } = string.Empty;

        [MaxLength(80), Column(TypeName = "nvarchar(80)")]
        public string? NativeName { get; set; }

        public short? CallingCode { get; set; }

        public byte PostalCodeLength { get; set; }

        public ICollection<CountryTranslation> Translations { get; set; } = new List<CountryTranslation>();
        public ICollection<Region> Regions { get; set; } = new List<Region>();
    }
}
