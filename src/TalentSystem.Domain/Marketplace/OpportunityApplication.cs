using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Marketplace;

public sealed class OpportunityApplication : AuditableDomainEntity
{
    public Guid MarketplaceOpportunityId { get; set; }

    public Guid EmployeeId { get; set; }

    public OpportunityApplicationStatus ApplicationStatus { get; set; } = OpportunityApplicationStatus.Submitted;

    public string? MotivationStatement { get; set; }

    public DateTime AppliedOnUtc { get; set; }

    public DateTime? ReviewedOnUtc { get; set; }

    public string? Notes { get; set; }

    public MarketplaceOpportunity MarketplaceOpportunity { get; set; } = null!;

    public Employee Employee { get; set; } = null!;
}
