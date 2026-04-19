using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Persistence.Configurations;

public sealed class IntelligenceRunConfiguration : IEntityTypeConfiguration<IntelligenceRun>
{
    public void Configure(EntityTypeBuilder<IntelligenceRun> builder)
    {
        builder.ToTable("IntelligenceRuns");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RunType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.StartedOnUtc).IsRequired();
        builder.Property(x => x.CompletedOnUtc);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.TotalInsightsGenerated).IsRequired();
        builder.Property(x => x.TotalRecommendationsGenerated).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany()
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder.HasIndex(x => x.RunType).HasFilter($"[{nameof(IntelligenceRun.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.PerformanceCycleId).HasFilter($"[{nameof(IntelligenceRun.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.StartedOnUtc).HasFilter($"[{nameof(IntelligenceRun.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.Status).HasFilter($"[{nameof(IntelligenceRun.RecordStatus)}] <> {deletedFilter}");
    }
}
