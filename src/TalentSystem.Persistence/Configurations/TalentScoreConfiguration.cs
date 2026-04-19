using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Scoring;

namespace TalentSystem.Persistence.Configurations;

public sealed class TalentScoreConfiguration : IEntityTypeConfiguration<TalentScore>
{
    public void Configure(EntityTypeBuilder<TalentScore> builder)
    {
        builder.ToTable("TalentScores");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PerformanceScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.PotentialScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.FinalScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.PerformanceWeight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.PotentialWeight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.CalculationVersion).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CalculatedOnUtc).IsRequired();
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

        builder
            .HasIndex(x => new { x.EmployeeId, x.PerformanceCycleId })
            .IsUnique()
            .HasFilter($"[{nameof(TalentScore.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => x.FinalScore);
    }
}
