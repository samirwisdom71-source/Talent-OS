using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Scoring;

public sealed class TalentScore : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public decimal PerformanceScore { get; set; }

    public decimal PotentialScore { get; set; }

    public decimal FinalScore { get; set; }

    public decimal PerformanceWeight { get; set; }

    public decimal PotentialWeight { get; set; }

    public string CalculationVersion { get; set; } = null!;

    public DateTime CalculatedOnUtc { get; set; }

    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;
}
