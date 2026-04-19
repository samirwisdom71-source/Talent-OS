using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class MarketplaceOpportunityFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public MarketplaceOpportunityStatus? Status { get; set; }

    public OpportunityType? OpportunityType { get; set; }

    public Guid? OrganizationUnitId { get; set; }
}
