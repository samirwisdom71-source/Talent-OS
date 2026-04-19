using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface ISuccessionAnalyticsService
{
    Task<Result<SuccessionAnalyticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
