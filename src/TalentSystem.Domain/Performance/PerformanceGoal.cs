using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Performance;

public sealed class PerformanceGoal : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string TitleAr { get; set; } = null!;

    public string TitleEn { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Weight { get; set; }

    public string? TargetValue { get; set; }

    public PerformanceGoalStatus Status { get; set; } = PerformanceGoalStatus.Draft;

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;
}
