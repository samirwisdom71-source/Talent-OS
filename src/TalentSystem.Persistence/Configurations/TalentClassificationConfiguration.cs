using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class TalentClassificationConfiguration : IEntityTypeConfiguration<TalentClassification>
{
    public void Configure(EntityTypeBuilder<TalentClassification> builder)
    {
        builder.ToTable("TalentClassifications");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PerformanceBand).HasConversion<byte>().IsRequired();
        builder.Property(x => x.PotentialBand).HasConversion<byte>().IsRequired();
        builder.Property(x => x.NineBoxCode).HasConversion<byte>().IsRequired();
        builder.Property(x => x.CategoryName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IsHighPotential).IsRequired();
        builder.Property(x => x.IsHighPerformer).IsRequired();
        builder.Property(x => x.ClassifiedOnUtc).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany()
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.TalentScore)
            .WithMany()
            .HasForeignKey(x => x.TalentScoreId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => new { x.EmployeeId, x.PerformanceCycleId })
            .IsUnique()
            .HasFilter($"[{nameof(TalentClassification.RecordStatus)}] <> {deletedFilter}");

        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => x.NineBoxCode);
        builder.HasIndex(x => x.IsHighPotential);
        builder.HasIndex(x => x.IsHighPerformer);
        builder.HasIndex(x => x.PerformanceBand);
        builder.HasIndex(x => x.PotentialBand);
    }
}
