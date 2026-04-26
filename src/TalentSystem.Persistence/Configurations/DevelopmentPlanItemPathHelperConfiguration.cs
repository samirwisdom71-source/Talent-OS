using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanItemPathHelperConfiguration : IEntityTypeConfiguration<DevelopmentPlanItemPathHelper>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlanItemPathHelper> builder)
    {
        builder.ToTable("DevelopmentPlanItemPathHelpers");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.HelperKind).HasConversion<byte>().IsRequired();

        builder
            .HasOne(x => x.Path)
            .WithMany(x => x.Helpers)
            .HasForeignKey(x => x.DevelopmentPlanItemPathId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DevelopmentPlanItemPathId);
        builder.HasIndex(x => new { x.DevelopmentPlanItemPathId, x.HelperKind, x.HelperEntityId });
    }
}
