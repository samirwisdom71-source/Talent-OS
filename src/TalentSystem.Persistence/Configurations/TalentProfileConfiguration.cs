using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Talent;

namespace TalentSystem.Persistence.Configurations;

public sealed class TalentProfileConfiguration : IEntityTypeConfiguration<TalentProfile>
{
    public void Configure(EntityTypeBuilder<TalentProfile> builder)
    {
        builder.ToTable("TalentProfiles");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Summary).HasMaxLength(4000);

        builder
            .HasOne(x => x.Employee)
            .WithOne(x => x.TalentProfile)
            .HasForeignKey<TalentProfile>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EmployeeId).IsUnique();
    }
}
