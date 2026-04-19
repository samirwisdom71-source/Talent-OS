using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.JobArchitecture;
using TalentSystem.Domain.Organizations;

namespace TalentSystem.Domain.Marketplace;

public sealed class MarketplaceOpportunity : AuditableDomainEntity
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public OpportunityType OpportunityType { get; set; }

    public Guid OrganizationUnitId { get; set; }

    public Guid? PositionId { get; set; }

    public string? RequiredCompetencySummary { get; set; }

    public MarketplaceOpportunityStatus Status { get; set; } = MarketplaceOpportunityStatus.Draft;

    public DateTime OpenDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? MaxApplicants { get; set; }

    public bool IsConfidential { get; set; }

    public string? Notes { get; set; }

    public OrganizationUnit OrganizationUnit { get; set; } = null!;

    public Position? Position { get; set; }

    public ICollection<OpportunityApplication> Applications { get; set; } = new List<OpportunityApplication>();
}
