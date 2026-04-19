using TalentSystem.Domain.Common;
using TalentSystem.Domain.Competencies;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlanItem : AuditableDomainEntity
{
    public Guid DevelopmentPlanId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DevelopmentItemType ItemType { get; set; }

    public Guid? RelatedCompetencyId { get; set; }

    public DateTime? TargetDate { get; set; }

    public DevelopmentItemStatus Status { get; set; } = DevelopmentItemStatus.NotStarted;

    public decimal ProgressPercentage { get; set; }

    public string? Notes { get; set; }

    public DevelopmentPlan DevelopmentPlan { get; set; } = null!;

    public Competency? RelatedCompetency { get; set; }
}
