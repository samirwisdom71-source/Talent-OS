using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Potential;

namespace TalentSystem.Persistence.Configurations;

public sealed class PotentialAssessmentConfiguration : IEntityTypeConfiguration<PotentialAssessment>
{
    public void Configure(EntityTypeBuilder<PotentialAssessment> builder)
    {
        builder.ToTable("PotentialAssessments");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AgilityScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.LeadershipScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.GrowthScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.MobilityScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.OverallPotentialScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.PotentialLevel).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Comments).HasMaxLength(4000);
        builder.Property(x => x.AssessedOnUtc);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AssessedByEmployee)
            .WithMany()
            .HasForeignKey(x => x.AssessedByEmployeeId)
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
            .HasFilter($"[{nameof(PotentialAssessment.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => x.PotentialLevel);
        builder.HasIndex(x => x.Status);
    }
}
