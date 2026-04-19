using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Interfaces;

public interface ITalentAnalyticsService
{
    Task<Result<TalentDistributionSummaryDto>> GetDistributionAsync(
        TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>> GetByCycleAsync(
        TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken = default);
}
