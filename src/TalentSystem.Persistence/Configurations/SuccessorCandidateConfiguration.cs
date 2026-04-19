using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;

namespace TalentSystem.Persistence.Configurations;

public sealed class SuccessorCandidateConfiguration : IEntityTypeConfiguration<SuccessorCandidate>
{
    public void Configure(EntityTypeBuilder<SuccessorCandidate> builder)
    {
        builder.ToTable("SuccessorCandidates");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReadinessLevel).HasConversion<byte>().IsRequired();
        builder.Property(x => x.RankOrder).IsRequired();
        builder.Property(x => x.IsPrimarySuccessor).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.SuccessionPlan)
            .WithMany(x => x.SuccessorCandidates)
            .HasForeignKey(x => x.SuccessionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.SuccessionPlanId, x.EmployeeId })
            .IsUnique()
            .HasFilter($"[{nameof(SuccessorCandidate.RecordStatus)}] <> {deletedFilter}");

        builder
            .HasIndex(x => new { x.SuccessionPlanId, x.RankOrder })
            .IsUnique()
            .HasDatabaseName("IX_SuccessorCandidates_SuccessionPlanId_RankOrder")
            .HasFilter($"[{nameof(SuccessorCandidate.RecordStatus)}] <> {deletedFilter}");

        builder
            .HasIndex(x => x.SuccessionPlanId)
            .IsUnique()
            .HasDatabaseName("IX_SuccessorCandidates_SinglePrimaryPerPlan")
            .HasFilter(
                $"[{nameof(SuccessorCandidate.IsPrimarySuccessor)}] = 1 AND [{nameof(SuccessorCandidate.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.ReadinessLevel);
        builder.HasIndex(x => x.RankOrder);
    }
}
