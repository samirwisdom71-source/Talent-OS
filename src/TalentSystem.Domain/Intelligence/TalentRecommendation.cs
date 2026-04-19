using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Intelligence;

public sealed class TalentRecommendation : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public RecommendationType RecommendationType { get; set; }

    public RecommendationPriority Priority { get; set; }

    public RecommendationSource Source { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string RecommendedAction { get; set; } = null!;

    public byte ConfidenceScore { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

    public TalentRecommendationStatus Status { get; set; } = TalentRecommendationStatus.Active;

    public DateTime GeneratedOnUtc { get; set; }

    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle? PerformanceCycle { get; set; }
}
