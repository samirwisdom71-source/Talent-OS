using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;

namespace TalentSystem.Persistence.Configurations;

public sealed class CriticalPositionConfiguration : IEntityTypeConfiguration<CriticalPosition>
{
    public void Configure(EntityTypeBuilder<CriticalPosition> builder)
    {
        builder.ToTable("CriticalPositions");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CriticalityLevel).HasConversion<byte>().IsRequired();
        builder.Property(x => x.RiskLevel).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        var activeRecord = (byte)RecordStatus.Active;

        builder
            .HasIndex(x => x.PositionId)
            .IsUnique()
            .HasDatabaseName("IX_CriticalPositions_SingleActivePerPosition")
            .HasFilter($"[{nameof(CriticalPosition.RecordStatus)}] = {activeRecord}");
    }
}
