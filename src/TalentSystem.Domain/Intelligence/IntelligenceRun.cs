using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Intelligence;

public sealed class IntelligenceRun : AuditableDomainEntity
{
    public IntelligenceRunType RunType { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public Guid? EmployeeId { get; set; }

    public DateTime StartedOnUtc { get; set; }

    public DateTime? CompletedOnUtc { get; set; }

    public IntelligenceRunStatus Status { get; set; } = IntelligenceRunStatus.Started;

    public int TotalInsightsGenerated { get; set; }

    public int TotalRecommendationsGenerated { get; set; }

    public string? Notes { get; set; }

    public PerformanceCycle? PerformanceCycle { get; set; }

    public Employee? Employee { get; set; }
}
