using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface IPerformanceAnalyticsService
{
    Task<Result<PerformanceAnalyticsSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceImpactSummaryDto>> GetImpactAsync(
        PerformanceImpactFilterRequest request,
        CancellationToken cancellationToken = default);
}
