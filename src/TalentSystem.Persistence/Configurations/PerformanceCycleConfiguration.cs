using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Persistence.Configurations;

public sealed class PerformanceCycleConfiguration : IEntityTypeConfiguration<PerformanceCycle>
{
    public void Configure(EntityTypeBuilder<PerformanceCycle> builder)
    {
        builder.ToTable("PerformanceCycles");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        builder.HasIndex(x => new { x.StartDate, x.EndDate });

        var activeStatus = (byte)PerformanceCycleStatus.Active;
        builder
            .HasIndex(x => x.Status)
            .HasDatabaseName("IX_PerformanceCycles_Status_Active")
            .HasFilter($"[{nameof(PerformanceCycle.Status)}] = {activeStatus}");
    }
}
