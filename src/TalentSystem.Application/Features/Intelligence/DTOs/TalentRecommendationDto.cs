using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Application.Features.Intelligence.DTOs;

public sealed class TalentRecommendationDto
{
    public Guid Id { get; init; }

    public Guid EmployeeId { get; init; }

    public Guid? PerformanceCycleId { get; init; }

    public RecommendationType RecommendationType { get; init; }

    public RecommendationPriority Priority { get; init; }

    public RecommendationSource Source { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;

    public string RecommendedAction { get; init; } = null!;

    public int ConfidenceScore { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public string? RelatedEntityType { get; init; }

    public TalentRecommendationStatus Status { get; init; }

    public DateTime GeneratedOnUtc { get; init; }

    public string? Notes { get; init; }
}
