using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class OpportunityApplicationDto
{
    public Guid Id { get; set; }

    public Guid MarketplaceOpportunityId { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeDisplayName { get; set; }

    public string? MarketplaceOpportunityTitle { get; set; }

    public OpportunityApplicationStatus ApplicationStatus { get; set; }

    public string? MotivationStatement { get; set; }

    public DateTime AppliedOnUtc { get; set; }

    public DateTime? ReviewedOnUtc { get; set; }

    public string? Notes { get; set; }
}
