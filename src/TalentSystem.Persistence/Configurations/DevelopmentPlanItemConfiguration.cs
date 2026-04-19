using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanItemConfiguration : IEntityTypeConfiguration<DevelopmentPlanItem>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlanItem> builder)
    {
        builder.ToTable("DevelopmentPlanItems");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.ItemType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.TargetDate);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.ProgressPercentage).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.DevelopmentPlan)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.DevelopmentPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.RelatedCompetency)
            .WithMany()
            .HasForeignKey(x => x.RelatedCompetencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.DevelopmentPlanId);
        builder.HasIndex(x => x.RelatedCompetencyId);
        builder.HasIndex(x => x.Status);
    }
}
