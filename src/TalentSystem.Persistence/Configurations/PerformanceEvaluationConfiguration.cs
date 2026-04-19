using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Persistence.Configurations;

public sealed class PerformanceEvaluationConfiguration : IEntityTypeConfiguration<PerformanceEvaluation>
{
    public void Configure(EntityTypeBuilder<PerformanceEvaluation> builder)
    {
        builder.ToTable("PerformanceEvaluations");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OverallScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.ManagerComments).HasMaxLength(4000);
        builder.Property(x => x.EmployeeComments).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.EvaluatedOnUtc);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.PerformanceCycleId);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.EmployeeId, x.PerformanceCycleId })
            .IsUnique()
            .HasFilter($"[{nameof(PerformanceEvaluation.RecordStatus)}] <> {deletedFilter}");
    }
}
