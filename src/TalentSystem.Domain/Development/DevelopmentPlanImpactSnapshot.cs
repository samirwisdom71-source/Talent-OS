using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlanImpactSnapshot : AuditableDomainEntity
{
    public Guid DevelopmentPlanId { get; set; }

    public DevelopmentImpactPhase Phase { get; set; }

    public DateTime RecordedOnUtc { get; set; }

    public string? SummaryNotes { get; set; }

    /// <summary>مؤشر رقمي اختياري (مثلاً متوسط جاهزية الكفاءات أو درجة مجمّعة).</summary>
    public decimal? MetricScore { get; set; }

    public DevelopmentPlan DevelopmentPlan { get; set; } = null!;
}
