using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Performance;

public sealed class PerformanceEvaluation : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public decimal OverallScore { get; set; }

    public string? ManagerComments { get; set; }

    public string? EmployeeComments { get; set; }

    public PerformanceEvaluationStatus Status { get; set; } = PerformanceEvaluationStatus.Draft;

    public DateTime? EvaluatedOnUtc { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;
}
