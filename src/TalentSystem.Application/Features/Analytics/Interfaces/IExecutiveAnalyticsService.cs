using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface IExecutiveAnalyticsService
{
    Task<Result<ExecutiveDashboardSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default);
}
