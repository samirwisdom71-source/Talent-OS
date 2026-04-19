using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Persistence.Configurations;

public sealed class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> builder)
    {
        builder.ToTable("PerformanceGoals");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Weight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.TargetValue).HasMaxLength(512);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        builder
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PerformanceCycle)
            .WithMany(x => x.Goals)
            .HasForeignKey(x => x.PerformanceCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => new { x.EmployeeId, x.PerformanceCycleId });
    }
}
