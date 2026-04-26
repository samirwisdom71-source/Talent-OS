using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanImpactSnapshotConfiguration : IEntityTypeConfiguration<DevelopmentPlanImpactSnapshot>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlanImpactSnapshot> builder)
    {
        builder.ToTable("DevelopmentPlanImpactSnapshots");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Phase).HasConversion<byte>().IsRequired();
        builder.Property(x => x.SummaryNotes).HasMaxLength(4000);
        builder.Property(x => x.MetricScore).HasPrecision(18, 4);

        builder
            .HasOne(x => x.DevelopmentPlan)
            .WithMany(x => x.ImpactSnapshots)
            .HasForeignKey(x => x.DevelopmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DevelopmentPlanId);

        var deletedFilter = (byte)RecordStatus.Deleted;
        builder
            .HasIndex(x => new { x.DevelopmentPlanId, x.Phase })
            .IsUnique()
            .HasDatabaseName("IX_DevelopmentPlanImpactSnapshots_Plan_Phase")
            .HasFilter($"[{nameof(DevelopmentPlanImpactSnapshot.RecordStatus)}] <> {deletedFilter}");
    }
}
