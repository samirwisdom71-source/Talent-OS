using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanLinkConfiguration : IEntityTypeConfiguration<DevelopmentPlanLink>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlanLink> builder)
    {
        builder.ToTable("DevelopmentPlanLinks");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LinkType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.LinkedEntityId).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.DevelopmentPlan)
            .WithMany(x => x.Links)
            .HasForeignKey(x => x.DevelopmentPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.DevelopmentPlanId, x.LinkType, x.LinkedEntityId })
            .IsUnique()
            .HasDatabaseName("IX_DevelopmentPlanLinks_Plan_Type_Entity")
            .HasFilter($"[{nameof(DevelopmentPlanLink.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.DevelopmentPlanId);
        builder.HasIndex(x => x.LinkType);
    }
}
