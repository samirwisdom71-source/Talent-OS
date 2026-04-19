using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class DevelopmentAnalyticsSummaryDto
{
    public int TotalDevelopmentPlans { get; init; }

    public int ActiveDevelopmentPlans { get; init; }

    public int CompletedDevelopmentPlans { get; init; }

    public int CancelledDevelopmentPlans { get; init; }

    public int TotalDevelopmentPlanItems { get; init; }

    public int CompletedDevelopmentPlanItems { get; init; }

    public int InProgressDevelopmentPlanItems { get; init; }

    /// <summary>
    /// Average progress percentage for plan items on active development plans that are not completed or cancelled.
    /// </summary>
    public decimal? AverageProgressPercentageActiveItems { get; init; }

    public IReadOnlyList<DevelopmentItemTypeBreakdownDto> ItemsByType { get; init; } =
        Array.Empty<DevelopmentItemTypeBreakdownDto>();
}

public sealed class DevelopmentItemTypeBreakdownDto
{
    public DevelopmentItemType ItemType { get; init; }

    public int ItemCount { get; init; }

    public int CompletedCount { get; init; }

    public int InProgressCount { get; init; }
}
