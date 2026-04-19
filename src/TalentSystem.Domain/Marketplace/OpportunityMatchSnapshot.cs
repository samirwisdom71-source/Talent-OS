using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Marketplace;

public sealed class OpportunityMatchSnapshot : AuditableDomainEntity
{
    public Guid MarketplaceOpportunityId { get; set; }

    public Guid EmployeeId { get; set; }

    public decimal MatchScore { get; set; }

    public OpportunityMatchLevel MatchLevel { get; set; }

    public string? Notes { get; set; }

    public DateTime CalculatedOnUtc { get; set; }

    public MarketplaceOpportunity MarketplaceOpportunity { get; set; } = null!;

    public Employee Employee { get; set; } = null!;
}
