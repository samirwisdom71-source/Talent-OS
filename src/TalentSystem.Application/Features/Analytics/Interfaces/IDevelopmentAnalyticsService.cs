using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface IDevelopmentAnalyticsService
{
    Task<Result<DevelopmentAnalyticsSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default);
}
