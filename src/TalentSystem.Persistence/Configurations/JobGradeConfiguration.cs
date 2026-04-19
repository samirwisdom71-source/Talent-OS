using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.JobArchitecture;

namespace TalentSystem.Persistence.Configurations;

public sealed class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
{
    public void Configure(EntityTypeBuilder<JobGrade> builder)
    {
        builder.ToTable("JobGrades");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Level).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
