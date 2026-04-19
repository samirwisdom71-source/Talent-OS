using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Potential;

namespace TalentSystem.Persistence.Configurations;

public sealed class PotentialAssessmentFactorConfiguration : IEntityTypeConfiguration<PotentialAssessmentFactor>
{
    public void Configure(EntityTypeBuilder<PotentialAssessmentFactor> builder)
    {
        builder.ToTable("PotentialAssessmentFactors");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FactorName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Score).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.Weight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.PotentialAssessment)
            .WithMany(x => x.Factors)
            .HasForeignKey(x => x.PotentialAssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PotentialAssessmentId);
    }
}
