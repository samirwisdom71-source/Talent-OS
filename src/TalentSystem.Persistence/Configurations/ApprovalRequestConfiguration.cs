using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Approvals;

namespace TalentSystem.Persistence.Configurations;

public sealed class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequests");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.RelatedEntityId).IsRequired();
        builder.Property(x => x.RequestedByUserId).IsRequired();
        builder.Property(x => x.CurrentApproverUserId);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.SubmittedOnUtc);
        builder.Property(x => x.CompletedOnUtc);
        builder.Property(x => x.Title).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000);
        builder.Property(x => x.Notes).HasMaxLength(4000);

        builder
            .HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.CurrentApproverUser)
            .WithMany()
            .HasForeignKey(x => x.CurrentApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RequestType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.RequestedByUserId);
        builder.HasIndex(x => x.CurrentApproverUserId);
        builder.HasIndex(x => x.SubmittedOnUtc);
    }
}
