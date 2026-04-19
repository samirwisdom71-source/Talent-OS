using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Competencies;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class CompetencyLevelConfiguration : IEntityTypeConfiguration<CompetencyLevel>
{
    public void Configure(EntityTypeBuilder<CompetencyLevel> builder)
    {
        builder.ToTable("CompetencyLevels");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NumericValue).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => x.NumericValue)
            .IsUnique()
            .HasFilter($"[{nameof(CompetencyLevel.RecordStatus)}] <> {deletedFilter}");
    }
}
