using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Competencies;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class JobCompetencyRequirementConfiguration : IEntityTypeConfiguration<JobCompetencyRequirement>
{
    public void Configure(EntityTypeBuilder<JobCompetencyRequirement> builder)
    {
        builder.ToTable("JobCompetencyRequirements");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Competency)
            .WithMany(x => x.JobRequirements)
            .HasForeignKey(x => x.CompetencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.RequiredLevel)
            .WithMany(x => x.JobRequirements)
            .HasForeignKey(x => x.RequiredLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.PositionId, x.CompetencyId })
            .IsUnique()
            .HasFilter($"[{nameof(JobCompetencyRequirement.RecordStatus)}] <> {deletedFilter}");
    }
}
