using TalentSystem.Domain.Common;
using TalentSystem.Domain.Organizations;

namespace TalentSystem.Domain.JobArchitecture;

public sealed class Position : AuditableDomainEntity
{
    public string TitleAr { get; set; } = null!;

    public string TitleEn { get; set; } = null!;

    public Guid OrganizationUnitId { get; set; }

    public Guid JobGradeId { get; set; }

    public OrganizationUnit OrganizationUnit { get; set; } = null!;

    public JobGrade JobGrade { get; set; } = null!;
}
