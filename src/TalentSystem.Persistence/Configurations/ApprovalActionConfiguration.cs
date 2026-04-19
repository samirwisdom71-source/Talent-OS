using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Approvals;

namespace TalentSystem.Persistence.Configurations;

public sealed class ApprovalActionConfiguration : IEntityTypeConfiguration<ApprovalAction>
{
    public void Configure(EntityTypeBuilder<ApprovalAction> builder)
    {
        builder.ToTable("ApprovalActions");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Comments).HasMaxLength(4000);
        builder.Property(x => x.ActionedOnUtc).IsRequired();

        builder
            .HasOne(x => x.ApprovalRequest)
            .WithMany(x => x.Actions)
            .HasForeignKey(x => x.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.ActionByUser)
            .WithMany()
            .HasForeignKey(x => x.ActionByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ApprovalRequestId);
        builder.HasIndex(x => x.ActionedOnUtc);
    }
}
