using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanItemPathConfiguration : IEntityTypeConfiguration<DevelopmentPlanItemPath>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlanItemPath> builder)
    {
        builder.ToTable("DevelopmentPlanItemPaths");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.AchievedImpactValue).HasPrecision(18, 4);

        builder
            .HasOne(x => x.DevelopmentPlanItem)
            .WithMany(x => x.Paths)
            .HasForeignKey(x => x.DevelopmentPlanItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DevelopmentPlanItemId);
        builder.HasIndex(x => new { x.DevelopmentPlanItemId, x.SortOrder });
    }
}
