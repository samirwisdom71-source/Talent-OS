using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface IMarketplaceAnalyticsService
{
    Task<Result<MarketplaceAnalyticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
