using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.ConfigureAuditableDomainEntity();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullNameAr).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FullNameEn).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();

        builder
            .HasOne(x => x.OrganizationUnit)
            .WithMany()
            .HasForeignKey(x => x.OrganizationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        var deletedFilter = (byte)RecordStatus.Deleted;

        builder
            .HasIndex(x => x.EmployeeNumber)
            .IsUnique()
            .HasFilter($"[{nameof(Employee.RecordStatus)}] <> {deletedFilter}");

        builder
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter($"[{nameof(Employee.RecordStatus)}] <> {deletedFilter}");
    }
}
