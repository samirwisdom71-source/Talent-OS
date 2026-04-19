using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Marketplace.Interfaces;

public interface IMarketplaceOpportunityService
{
    Task<Result<MarketplaceOpportunityDto>> CreateAsync(
        CreateMarketplaceOpportunityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<MarketplaceOpportunityDto>> UpdateAsync(
        Guid id,
        UpdateMarketplaceOpportunityRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<MarketplaceOpportunityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<MarketplaceOpportunityDto>>> GetPagedAsync(
        MarketplaceOpportunityFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<MarketplaceOpportunityDto>> OpenAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<MarketplaceOpportunityDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<MarketplaceOpportunityDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
