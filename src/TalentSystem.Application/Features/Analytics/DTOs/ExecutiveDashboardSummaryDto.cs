using TalentSystem.Domain.Enums;

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

    public int TotalPerformanceGoals { get; init; }

    public int CompletedPerformanceGoals { get; init; }

    public int FinalizedEvaluationCount { get; init; }

    public int PendingApprovalCount { get; init; }

    public int TalentInsightCount { get; init; }

    public int TalentRecommendationCount { get; init; }

    public int TotalMarketplaceApplicationCount { get; init; }

    public IReadOnlyList<NineBoxDistributionItemDto> NineBoxDistribution { get; init; } =
        Array.Empty<NineBoxDistributionItemDto>();

    public IReadOnlyList<EnumCountDto<PerformanceBand>> ByPerformanceBand { get; init; } =
        Array.Empty<EnumCountDto<PerformanceBand>>();

    public IReadOnlyList<EnumCountDto<PotentialBand>> ByPotentialBand { get; init; } =
        Array.Empty<EnumCountDto<PotentialBand>>();

    public IReadOnlyList<NamedCountDto> TopTalentCategories { get; init; } = Array.Empty<NamedCountDto>();

    public IReadOnlyList<ExecutiveCycleSnapshotDto> PerformanceByCycle { get; init; } =
        Array.Empty<ExecutiveCycleSnapshotDto>();

    public IReadOnlyList<ExecutiveCodeCountDto> DevelopmentItemsByType { get; init; } =
        Array.Empty<ExecutiveCodeCountDto>();

    public IReadOnlyList<ExecutiveCodeCountDto> MarketplaceOpportunitiesByType { get; init; } =
        Array.Empty<ExecutiveCodeCountDto>();

    public IReadOnlyList<EnumCountDto<ReadinessLevel>> SuccessorReadiness { get; init; } =
        Array.Empty<EnumCountDto<ReadinessLevel>>();
}

public sealed class ExecutiveCycleSnapshotDto
{
    public Guid PerformanceCycleId { get; init; }

    public string PerformanceCycleNameEn { get; init; } = string.Empty;

    public string PerformanceCycleNameAr { get; init; } = string.Empty;

    public int FinalizedEvaluations { get; init; }
}

public sealed class ExecutiveCodeCountDto
{
    public int Code { get; init; }

    public int Count { get; init; }
}
