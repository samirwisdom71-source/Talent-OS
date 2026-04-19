using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Approvals;

namespace TalentSystem.Persistence.Configurations;

public sealed class ApprovalAssignmentConfiguration : IEntityTypeConfiguration<ApprovalAssignment>
{
    public void Configure(EntityTypeBuilder<ApprovalAssignment> builder)
    {
        builder.ToTable("ApprovalAssignments");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssignedOnUtc).IsRequired();
        builder.Property(x => x.IsCurrent).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.ApprovalRequest)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AssignedByUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => x.ApprovalRequestId)
            .IsUnique()
            .HasFilter(
                $"[{nameof(ApprovalAssignment.IsCurrent)}] = CAST(1 AS bit) AND [{nameof(ApprovalAssignment.RecordStatus)}] <> {deletedFilter}");
    }
}
