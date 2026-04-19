using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Competencies;

namespace TalentSystem.Persistence.Configurations;

public sealed class CompetencyCategoryConfiguration : IEntityTypeConfiguration<CompetencyCategory>
{
    public void Configure(EntityTypeBuilder<CompetencyCategory> builder)
    {
        builder.ToTable("CompetencyCategories");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => x.NameEn);
    }
}
