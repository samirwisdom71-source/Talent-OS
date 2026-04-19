namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class ApplyOpportunityRequest
{
    public Guid MarketplaceOpportunityId { get; set; }

    public Guid EmployeeId { get; set; }

    public string? MotivationStatement { get; set; }
}
