using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Competencies;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class CompetencyConfiguration : IEntityTypeConfiguration<Competency>
{
    public void Configure(EntityTypeBuilder<Competency> builder)
    {
        builder.ToTable("Competencies");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder
            .HasOne(x => x.CompetencyCategory)
            .WithMany(x => x.Competencies)
            .HasForeignKey(x => x.CompetencyCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter($"[{nameof(Competency.RecordStatus)}] <> {deletedFilter}");
    }
}
