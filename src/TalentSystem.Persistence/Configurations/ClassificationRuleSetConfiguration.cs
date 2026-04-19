using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class ClassificationRuleSetConfiguration : IEntityTypeConfiguration<ClassificationRuleSet>
{
    public void Configure(EntityTypeBuilder<ClassificationRuleSet> builder)
    {
        builder.ToTable("ClassificationRuleSets");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LowThreshold).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.HighThreshold).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.EffectiveFromUtc).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        var deletedFilter = (byte)RecordStatus.Deleted;
        var activeRecord = (byte)RecordStatus.Active;

        builder
            .HasIndex(x => x.Version)
            .IsUnique()
            .HasFilter($"[{nameof(ClassificationRuleSet.RecordStatus)}] <> {deletedFilter}");

        builder
            .HasIndex(x => x.RecordStatus)
            .IsUnique()
            .HasDatabaseName("IX_ClassificationRuleSets_SingleActiveRuleSet")
            .HasFilter($"[{nameof(ClassificationRuleSet.RecordStatus)}] = {activeRecord}");
    }
}
