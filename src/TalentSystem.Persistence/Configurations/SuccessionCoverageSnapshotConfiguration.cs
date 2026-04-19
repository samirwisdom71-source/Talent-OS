using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;

namespace TalentSystem.Persistence.Configurations;

public sealed class SuccessionCoverageSnapshotConfiguration : IEntityTypeConfiguration<SuccessionCoverageSnapshot>
{
    public void Configure(EntityTypeBuilder<SuccessionCoverageSnapshot> builder)
    {
        builder.ToTable("SuccessionCoverageSnapshots");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalCandidates).IsRequired();
        builder.Property(x => x.HasReadyNow).IsRequired();
        builder.Property(x => x.HasPrimarySuccessor).IsRequired();
        builder.Property(x => x.CoverageScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.CalculatedOnUtc).IsRequired();

        builder
            .HasOne(x => x.SuccessionPlan)
            .WithMany()
            .HasForeignKey(x => x.SuccessionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => x.SuccessionPlanId)
            .IsUnique()
            .HasDatabaseName("IX_SuccessionCoverageSnapshots_OnePerPlan")
            .HasFilter($"[{nameof(SuccessionCoverageSnapshot.RecordStatus)}] <> {deletedFilter}");
    }
}
