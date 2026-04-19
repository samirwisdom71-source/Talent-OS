using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;

namespace TalentSystem.Persistence.Configurations;

public sealed class SuccessionPlanConfiguration : IEntityTypeConfiguration<SuccessionPlan>
{
    public void Configure(EntityTypeBuilder<SuccessionPlan> builder)
    {
        builder.ToTable("SuccessionPlans");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.CriticalPosition)
            .WithMany(x => x.SuccessionPlans)
            .HasForeignKey(x => x.CriticalPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany()
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.CriticalPositionId, x.PerformanceCycleId })
            .IsUnique()
            .HasFilter($"[{nameof(SuccessionPlan.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => x.Status);
    }
}
