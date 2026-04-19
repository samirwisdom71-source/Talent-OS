using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class CreateMarketplaceOpportunityRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OpportunityType OpportunityType { get; set; }

    public Guid OrganizationUnitId { get; set; }

    public Guid? PositionId { get; set; }

    public string? RequiredCompetencySummary { get; set; }

    public DateTime OpenDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? MaxApplicants { get; set; }

    public bool IsConfidential { get; set; }

    public string? Notes { get; set; }
}
