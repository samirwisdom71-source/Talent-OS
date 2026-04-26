using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlanItemPathHelper : AuditableDomainEntity
{
    public Guid DevelopmentPlanItemPathId { get; set; }

    public PathHelperKind HelperKind { get; set; }

    public Guid HelperEntityId { get; set; }

    public DevelopmentPlanItemPath Path { get; set; } = null!;
}
