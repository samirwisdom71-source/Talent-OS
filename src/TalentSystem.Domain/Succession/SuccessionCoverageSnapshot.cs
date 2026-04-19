using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Succession;

public sealed class SuccessionCoverageSnapshot : AuditableDomainEntity
{
    public Guid SuccessionPlanId { get; set; }

    public int TotalCandidates { get; set; }

    public bool HasReadyNow { get; set; }

    public bool HasPrimarySuccessor { get; set; }

    public decimal CoverageScore { get; set; }

    public DateTime CalculatedOnUtc { get; set; }

    public SuccessionPlan SuccessionPlan { get; set; } = null!;
}
