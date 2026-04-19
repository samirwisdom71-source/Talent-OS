using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Scoring;

namespace TalentSystem.Persistence.Configurations;

public sealed class ScoringPolicyConfiguration : IEntityTypeConfiguration<ScoringPolicy>
{
    public void Configure(EntityTypeBuilder<ScoringPolicy> builder)
    {
        builder.ToTable("ScoringPolicies");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PerformanceWeight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.PotentialWeight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        var deletedFilter = (byte)RecordStatus.Deleted;
        var activeRecord = (byte)RecordStatus.Active;

        builder
            .HasIndex(x => x.Version)
            .IsUnique()
            .HasFilter($"[{nameof(ScoringPolicy.RecordStatus)}] <> {deletedFilter}");

        builder
            .HasIndex(x => x.RecordStatus)
            .IsUnique()
            .HasDatabaseName("IX_ScoringPolicies_SingleActivePolicy")
            .HasFilter($"[{nameof(ScoringPolicy.RecordStatus)}] = {activeRecord}");
    }
}
