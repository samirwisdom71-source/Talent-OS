using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class DevelopmentPlanConfiguration : IEntityTypeConfiguration<DevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlan> builder)
    {
        builder.ToTable("DevelopmentPlans");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanTitle).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SourceType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.TargetCompletionDate);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.ApprovedOnUtc);

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
            .HasOne(x => x.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.PerformanceCycleId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SourceType);
    }
}
