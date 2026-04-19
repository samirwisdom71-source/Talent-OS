using TalentSystem.Domain.Common;
using TalentSystem.Domain.JobArchitecture;
using TalentSystem.Domain.Organizations;
using TalentSystem.Domain.Talent;

namespace TalentSystem.Domain.Employees;

public sealed class Employee : AuditableDomainEntity
{
    public string EmployeeNumber { get; set; } = null!;

    public string FullNameAr { get; set; } = null!;

    public string FullNameEn { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Guid OrganizationUnitId { get; set; }

    public Guid PositionId { get; set; }

    public OrganizationUnit OrganizationUnit { get; set; } = null!;

    public Position Position { get; set; } = null!;

    public TalentProfile? TalentProfile { get; set; }
}
