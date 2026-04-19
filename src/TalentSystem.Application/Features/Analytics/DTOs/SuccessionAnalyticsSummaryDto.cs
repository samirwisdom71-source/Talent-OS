using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class SuccessionAnalyticsSummaryDto
{
    public int TotalCriticalPositions { get; init; }

    /// <summary>
    /// Critical positions that currently have at least one active succession plan.
    /// </summary>
    public int ActiveCriticalPositions { get; init; }

    public int TotalSuccessionPlans { get; init; }

    public int ActiveSuccessionPlans { get; init; }

    public int PlansWithReadyNowSuccessor { get; init; }

    public int PlansWithPrimarySuccessor { get; init; }

    public decimal? AverageCoverageScore { get; init; }

    public IReadOnlyList<EnumCountDto<ReadinessLevel>> SuccessorReadinessBreakdown { get; init; } =
        Array.Empty<EnumCountDto<ReadinessLevel>>();
}
