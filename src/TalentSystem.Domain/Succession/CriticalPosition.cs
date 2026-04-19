using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.JobArchitecture;

namespace TalentSystem.Domain.Succession;

public sealed class CriticalPosition : AuditableDomainEntity
{
    public Guid PositionId { get; set; }

    public CriticalityLevel CriticalityLevel { get; set; }

    public SuccessionRiskLevel RiskLevel { get; set; }

    public string? Notes { get; set; }

    public Position Position { get; set; } = null!;

    public ICollection<SuccessionPlan> SuccessionPlans { get; set; } = new List<SuccessionPlan>();
}
