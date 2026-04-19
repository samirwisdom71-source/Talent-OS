using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class OpportunityApplicationFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? MarketplaceOpportunityId { get; set; }

    public Guid? EmployeeId { get; set; }
}
