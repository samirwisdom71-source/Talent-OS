using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Marketplace.DTOs;

public sealed class MarketplaceOpportunityDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OpportunityType OpportunityType { get; set; }

    public Guid OrganizationUnitId { get; set; }

    public string? OrganizationUnitName { get; set; }

    public Guid? PositionId { get; set; }

    public string? PositionTitle { get; set; }

    public string? RequiredCompetencySummary { get; set; }

    public MarketplaceOpportunityStatus Status { get; set; }

    public DateTime OpenDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? MaxApplicants { get; set; }

    public bool IsConfidential { get; set; }

    public string? Notes { get; set; }
}
