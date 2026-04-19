using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;

namespace TalentSystem.Persistence.Configurations;

public sealed class MarketplaceOpportunityConfiguration : IEntityTypeConfiguration<MarketplaceOpportunity>
{
    public void Configure(EntityTypeBuilder<MarketplaceOpportunity> builder)
    {
        builder.ToTable("MarketplaceOpportunities");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.OpportunityType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.RequiredCompetencySummary).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.OpenDate).IsRequired();
        builder.Property(x => x.CloseDate);
        builder.Property(x => x.MaxApplicants);
        builder.Property(x => x.IsConfidential).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.OrganizationUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OpportunityType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OrganizationUnitId);
        builder.HasIndex(x => x.PositionId);
        builder.HasIndex(x => x.OpenDate);
    }
}
