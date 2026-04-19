using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Succession;

public sealed class SuccessionPlan : AuditableDomainEntity
{
    public Guid CriticalPositionId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanName { get; set; } = null!;

    public SuccessionPlanStatus Status { get; set; } = SuccessionPlanStatus.Draft;

    public string? Notes { get; set; }

    public CriticalPosition CriticalPosition { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;

    public ICollection<SuccessorCandidate> SuccessorCandidates { get; set; } = new List<SuccessorCandidate>();
}
