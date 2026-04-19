using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;

namespace TalentSystem.Persistence.Configurations;

public sealed class OpportunityMatchSnapshotConfiguration : IEntityTypeConfiguration<OpportunityMatchSnapshot>
{
    public void Configure(EntityTypeBuilder<OpportunityMatchSnapshot> builder)
    {
        builder.ToTable("OpportunityMatchSnapshots");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MatchScore).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.MatchLevel).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CalculatedOnUtc).IsRequired();

        builder
            .HasOne(x => x.MarketplaceOpportunity)
            .WithMany()
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
            .HasDatabaseName("IX_OpportunityMatchSnapshots_Opportunity_Employee")
            .HasFilter($"[{nameof(OpportunityMatchSnapshot.RecordStatus)}] <> {deletedFilter}");
    }
}
