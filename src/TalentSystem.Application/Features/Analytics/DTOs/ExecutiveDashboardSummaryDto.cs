namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class ExecutiveDashboardSummaryDto
{
    public int TotalEmployees { get; init; }

    public int TotalTalentScores { get; init; }

    public int TotalTalentClassifications { get; init; }

    public int HighPotentialCount { get; init; }

    public int HighPerformerCount { get; init; }

    /// <summary>
    /// Classifications in the top-right 9-box cell (Strategic Leader / Box9).
    /// </summary>
    public int StrategicLeaderCount { get; init; }

    public int ActivePerformanceCycleCount { get; init; }

    public int ActiveSuccessionPlanCount { get; init; }

    public int OpenMarketplaceOpportunityCount { get; init; }

    public int ActiveDevelopmentPlanCount { get; init; }
}
