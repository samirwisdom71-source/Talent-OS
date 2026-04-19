using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Potential;

public sealed class PotentialAssessmentFactor : AuditableDomainEntity
{
    public Guid PotentialAssessmentId { get; set; }

    public string FactorName { get; set; } = null!;

    public decimal Score { get; set; }

    public decimal Weight { get; set; }

    public string? Notes { get; set; }

    public PotentialAssessment PotentialAssessment { get; set; } = null!;
}
