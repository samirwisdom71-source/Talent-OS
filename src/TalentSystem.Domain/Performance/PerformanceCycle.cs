using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Performance;

public sealed class PerformanceCycle : AuditableDomainEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public PerformanceCycleStatus Status { get; set; } = PerformanceCycleStatus.Draft;

    public string? Description { get; set; }

    public ICollection<PerformanceGoal> Goals { get; set; } = new List<PerformanceGoal>();

    public ICollection<PerformanceEvaluation> Evaluations { get; set; } = new List<PerformanceEvaluation>();
}
