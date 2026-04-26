using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlanItemPath : AuditableDomainEntity
{
    public Guid DevelopmentPlanItemId { get; set; }

    public int SortOrder { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? PlannedStartUtc { get; set; }

    public DateTime? PlannedEndUtc { get; set; }

    public DevelopmentItemStatus Status { get; set; } = DevelopmentItemStatus.NotStarted;

    /// <summary>
    /// قيمة الأثر المحقق من المسار (تُعاد حسابها عند إكمال مسارات أخرى في الخطة وفق إجمالي المسارات النشطة).
    /// </summary>
    public decimal? AchievedImpactValue { get; set; }

    public DevelopmentPlanItem DevelopmentPlanItem { get; set; } = null!;

    public ICollection<DevelopmentPlanItemPathHelper> Helpers { get; set; } = new List<DevelopmentPlanItemPathHelper>();
}
