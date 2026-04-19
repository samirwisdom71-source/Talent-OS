namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class PerformanceAnalyticsSummaryDto
{
    public int TotalPerformanceCycles { get; init; }

    public int ActivePerformanceCycles { get; init; }

    public int TotalGoals { get; init; }

    public int CompletedGoals { get; init; }

    public int TotalEvaluations { get; init; }

    public int FinalizedEvaluations { get; init; }

    public decimal? AverageOverallEvaluationScoreFinalized { get; init; }

    public IReadOnlyList<PerformanceCycleAnalyticsBreakdownDto> BreakdownByCycle { get; init; } =
        Array.Empty<PerformanceCycleAnalyticsBreakdownDto>();
}

public sealed class PerformanceCycleAnalyticsBreakdownDto
{
    public Guid PerformanceCycleId { get; init; }

    public string PerformanceCycleNameEn { get; init; } = string.Empty;

    public int TotalGoals { get; init; }

    public int CompletedGoals { get; init; }

    public int TotalEvaluations { get; init; }

    public int FinalizedEvaluations { get; init; }

    public decimal? AverageFinalizedOverallScore { get; init; }
}
