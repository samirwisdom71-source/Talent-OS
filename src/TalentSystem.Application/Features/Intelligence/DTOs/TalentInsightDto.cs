using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Application.Features.Intelligence.DTOs;

public sealed class TalentInsightDto
{
    public Guid Id { get; init; }

    public Guid? EmployeeId { get; init; }

    public Guid? PerformanceCycleId { get; init; }

    public InsightType InsightType { get; init; }

    public InsightSeverity Severity { get; init; }

    public InsightSource Source { get; init; }

    public string Title { get; init; } = null!;

    public string Summary { get; init; } = null!;

    public int ConfidenceScore { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public string? RelatedEntityType { get; init; }

    public TalentInsightStatus Status { get; init; }

    public DateTime GeneratedOnUtc { get; init; }

    public string? Notes { get; init; }
}
