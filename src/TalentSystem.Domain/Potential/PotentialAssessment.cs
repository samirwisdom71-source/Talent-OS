using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Potential;

public sealed class PotentialAssessment : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public Guid AssessedByEmployeeId { get; set; }

    public decimal AgilityScore { get; set; }

    public decimal LeadershipScore { get; set; }

    public decimal GrowthScore { get; set; }

    public decimal MobilityScore { get; set; }

    public decimal OverallPotentialScore { get; set; }

    public PotentialLevel PotentialLevel { get; set; }

    public string? Comments { get; set; }

    public PotentialAssessmentStatus Status { get; set; } = PotentialAssessmentStatus.Draft;

    public DateTime? AssessedOnUtc { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;

    public Employee AssessedByEmployee { get; set; } = null!;

    public ICollection<PotentialAssessmentFactor> Factors { get; set; } = new List<PotentialAssessmentFactor>();
}
