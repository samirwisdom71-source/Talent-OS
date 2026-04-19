using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Succession;

public sealed class SuccessorCandidate : AuditableDomainEntity
{
    public Guid SuccessionPlanId { get; set; }

    public Guid EmployeeId { get; set; }

    public ReadinessLevel ReadinessLevel { get; set; }

    public int RankOrder { get; set; }

    public bool IsPrimarySuccessor { get; set; }

    public string? Notes { get; set; }

    public SuccessionPlan SuccessionPlan { get; set; } = null!;

    public Employee Employee { get; set; } = null!;
}
