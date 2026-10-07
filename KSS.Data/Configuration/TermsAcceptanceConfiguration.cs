using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KSS.Entity;

namespace KSS.Data.Configuration
{
    public class TermsAcceptanceConfiguration : IEntityTypeConfiguration<TermsAcceptance>
    {
        public void Configure(EntityTypeBuilder<TermsAcceptance> b)
        {
            b.Property(x => x.TermsVersion).HasMaxLength(32).IsUnicode(true);
            b.Property(x => x.AcceptedAt).HasColumnType("datetime2(7)");
            b.Property(x => x.CreatedAt).HasColumnType("datetime2(7)");

            // Mirrors UQ_TermsAcceptance_PersonModuleVersion, so a test provider enforces it too.
            b.HasIndex(x => new { x.PersonId, x.ModuleId, x.TermsVersion })
                .IsUnique()
                .HasDatabaseName("UQ_TermsAcceptance_PersonModuleVersion");

            // One-directional: no navigation on Module, so Module's existing reads and payloads are unchanged.
            // Restrict mirrors the table's NO ACTION.
            b.HasOne<Module>().WithMany().HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
