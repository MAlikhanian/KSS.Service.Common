using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    /// <summary>
    /// One acceptance of one version of an application's terms by one person.
    /// Rows are written once and never updated or deleted by the service.
    /// UpdatedBy/UpdatedAt/DeletedBy/DeletedAt exist in the table but are deliberately NOT mapped:
    /// the context stamps any property named UpdatedAt on insert, and the table requires that pair
    /// to be both null or both set. Unmapped, they are written as NULL.
    /// </summary>
    [Table("TermsAcceptance")]
    public class TermsAcceptance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }              // UUIDv7, set by the service

        public Guid PersonId { get; set; }        // from the caller's token; Person is another database (no FK)

        public Guid ModuleId { get; set; }        // FK to Module.Id

        [Required, MaxLength(32)]
        public string TermsVersion { get; set; } = string.Empty;

        public DateTime AcceptedAt { get; set; }  // UTC

        public Guid CreatedBy { get; set; }       // the accepting person

        public DateTime CreatedAt { get; set; }   // stamped by the context
    }
}
