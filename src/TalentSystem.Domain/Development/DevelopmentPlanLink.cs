using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlanLink : AuditableDomainEntity
{
    public Guid DevelopmentPlanId { get; set; }

    public DevelopmentPlanLinkType LinkType { get; set; }

    public Guid LinkedEntityId { get; set; }

    public string? Notes { get; set; }

    public DevelopmentPlan DevelopmentPlan { get; set; } = null!;
}
