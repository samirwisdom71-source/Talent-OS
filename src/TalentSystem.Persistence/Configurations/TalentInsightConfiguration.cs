using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Persistence.Configurations;

public sealed class TalentInsightConfiguration : IEntityTypeConfiguration<TalentInsight>
{
    public void Configure(EntityTypeBuilder<TalentInsight> builder)
    {
        builder.ToTable("TalentInsights");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InsightType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Severity).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Source).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ConfidenceScore).IsRequired();
        builder.Property(x => x.RelatedEntityType).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.GeneratedOnUtc).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany()
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder.HasIndex(x => x.EmployeeId).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.PerformanceCycleId).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.InsightType).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.Severity).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.Status).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
        builder.HasIndex(x => x.GeneratedOnUtc).HasFilter($"[{nameof(TalentInsight.RecordStatus)}] <> {deletedFilter}");
    }
}
