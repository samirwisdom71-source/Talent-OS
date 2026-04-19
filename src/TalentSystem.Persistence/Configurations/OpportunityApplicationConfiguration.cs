using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;

namespace TalentSystem.Persistence.Configurations;

public sealed class OpportunityApplicationConfiguration : IEntityTypeConfiguration<OpportunityApplication>
{
    public void Configure(EntityTypeBuilder<OpportunityApplication> builder)
    {
        builder.ToTable("OpportunityApplications");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationStatus).HasConversion<byte>().IsRequired();
        builder.Property(x => x.MotivationStatement).HasMaxLength(4000);
        builder.Property(x => x.AppliedOnUtc).IsRequired();
        builder.Property(x => x.ReviewedOnUtc);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.MarketplaceOpportunity)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.MarketplaceOpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.MarketplaceOpportunityId, x.EmployeeId })
            .IsUnique()
            .HasFilter($"[{nameof(OpportunityApplication.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.ApplicationStatus);
        builder.HasIndex(x => x.AppliedOnUtc);
    }
}
