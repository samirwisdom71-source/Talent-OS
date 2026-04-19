using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Domain.Scoring;

namespace TalentSystem.Domain.Classification;

public sealed class TalentClassification : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public Guid TalentScoreId { get; set; }

    public PerformanceBand PerformanceBand { get; set; }

    public PotentialBand PotentialBand { get; set; }

    public NineBoxCode NineBoxCode { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool IsHighPotential { get; set; }

    public bool IsHighPerformer { get; set; }

    public DateTime ClassifiedOnUtc { get; set; }

    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;

    public TalentScore TalentScore { get; set; } = null!;
}
