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

public sealed class PerformanceImpactFilterRequest
{
    public DateTime? BeforeFromUtc { get; init; }

    public DateTime? BeforeToUtc { get; init; }

    public DateTime? AfterFromUtc { get; init; }

    public DateTime? AfterToUtc { get; init; }
}

public sealed class PerformanceImpactSummaryDto
{
    public DateTime BeforeFromUtc { get; init; }

    public DateTime BeforeToUtc { get; init; }

    public DateTime AfterFromUtc { get; init; }

    public DateTime AfterToUtc { get; init; }

    public int BeforeFinalizedEvaluations { get; init; }

    public int AfterFinalizedEvaluations { get; init; }

    public decimal? BeforeAverageScore { get; init; }

    public decimal? AfterAverageScore { get; init; }

    public decimal? ScoreDelta { get; init; }

    public decimal BeforeGoalCompletionRate { get; init; }

    public decimal AfterGoalCompletionRate { get; init; }

    public decimal GoalCompletionRateDelta { get; init; }

    /// <summary>Development plans whose <c>CreatedOnUtc</c> falls in period 1.</summary>
    public int BeforeDevelopmentPlansCreated { get; init; }

    /// <summary>Development plans whose <c>CreatedOnUtc</c> falls in period 2.</summary>
    public int AfterDevelopmentPlansCreated { get; init; }

    public int BeforeSuccessionPlansCreated { get; init; }

    public int AfterSuccessionPlansCreated { get; init; }

    /// <summary>Marketplace applications with <c>AppliedOnUtc</c> in period 1.</summary>
    public int BeforeMarketplaceApplications { get; init; }

    /// <summary>Marketplace applications with <c>AppliedOnUtc</c> in period 2.</summary>
    public int AfterMarketplaceApplications { get; init; }

    /// <summary>Talent classifications with <c>ClassifiedOnUtc</c> in period 1.</summary>
    public int BeforeTalentClassifications { get; init; }

    /// <summary>Talent classifications with <c>ClassifiedOnUtc</c> in period 2.</summary>
    public int AfterTalentClassifications { get; init; }
}

public sealed class PerformanceCycleAnalyticsBreakdownDto
{
    public Guid PerformanceCycleId { get; init; }

    public string PerformanceCycleNameEn { get; init; } = string.Empty;

    public string PerformanceCycleNameAr { get; init; } = string.Empty;

    public int TotalGoals { get; init; }

    public int CompletedGoals { get; init; }

    public int TotalEvaluations { get; init; }

    public int FinalizedEvaluations { get; init; }

    public decimal? AverageFinalizedOverallScore { get; init; }
}
