using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Intelligence;

public sealed class TalentInsight : AuditableDomainEntity
{
    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public InsightType InsightType { get; set; }

    public InsightSeverity Severity { get; set; }

    public InsightSource Source { get; set; }

    public string Title { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public byte ConfidenceScore { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

    public TalentInsightStatus Status { get; set; } = TalentInsightStatus.Active;

    public DateTime GeneratedOnUtc { get; set; }

    public string? Notes { get; set; }

    public Employee? Employee { get; set; }

    public PerformanceCycle? PerformanceCycle { get; set; }
}
