using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class MarketplaceAnalyticsSummaryDto
{
    public int TotalMarketplaceOpportunities { get; init; }

    public int DraftOpportunities { get; init; }

    public int OpenOpportunities { get; init; }

    public int ClosedOpportunities { get; init; }

    public int CancelledOpportunities { get; init; }

    public int ArchivedOpportunities { get; init; }

    public int TotalApplications { get; init; }

    public int SubmittedApplications { get; init; }

    public int UnderReviewApplications { get; init; }

    public int ShortlistedApplications { get; init; }

    public int AcceptedApplications { get; init; }

    public int RejectedApplications { get; init; }

    public int WithdrawnApplications { get; init; }

    public decimal? AverageMatchScore { get; init; }

    public IReadOnlyList<OpportunityTypeBreakdownDto> OpportunitiesByType { get; init; } =
        Array.Empty<OpportunityTypeBreakdownDto>();
}

public sealed class OpportunityTypeBreakdownDto
{
    public OpportunityType OpportunityType { get; init; }

    public int OpportunityCount { get; init; }

    public int OpenCount { get; init; }
}
