using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.JobArchitecture;

namespace TalentSystem.Persistence.Configurations;

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(256).IsRequired();

        builder
            .HasOne(x => x.OrganizationUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.JobGrade)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.JobGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationUnitId, x.TitleEn });
    }
}
