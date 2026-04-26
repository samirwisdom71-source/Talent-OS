using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class MarketplaceAnalyticsService : IMarketplaceAnalyticsService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<AnalyticsDateRangeFilter> _dateRangeValidator;

    public MarketplaceAnalyticsService(
        TalentDbContext db,
        IValidator<AnalyticsDateRangeFilter> dateRangeValidator)
    {
        _db = db;
        _dateRangeValidator = dateRangeValidator;
    }

    public async Task<Result<MarketplaceAnalyticsSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var guard = await AnalyticsDateRangeGuard.ValidateAsync(_dateRangeValidator, dateRange, cancellationToken)
            .ConfigureAwait(false);
        if (guard.IsFailure)
        {
            return Result<MarketplaceAnalyticsSummaryDto>.Fail(guard.Errors, guard.FailureCode);
        }

        var useRange = dateRange?.FromUtc is not null && dateRange.ToUtc is not null;
        var fromUtc = dateRange?.FromUtc ?? DateTime.MinValue;
        var toUtc = dateRange?.ToUtc ?? DateTime.MaxValue;

        var opps = _db.MarketplaceOpportunities.AsNoTracking();
        var apps = _db.OpportunityApplications.AsNoTracking();
        var snapshots = _db.OpportunityMatchSnapshots.AsNoTracking();

        if (useRange)
        {
            opps = opps.Where(o => o.CreatedOnUtc >= fromUtc && o.CreatedOnUtc <= toUtc);
            apps = apps.Where(a => a.AppliedOnUtc >= fromUtc && a.AppliedOnUtc <= toUtc);
            snapshots = snapshots.Where(s => s.CreatedOnUtc >= fromUtc && s.CreatedOnUtc <= toUtc);
        }

        var totalOpps = await opps.CountAsync(cancellationToken).ConfigureAwait(false);
        var draft = await opps.CountAsync(o => o.Status == MarketplaceOpportunityStatus.Draft, cancellationToken)
            .ConfigureAwait(false);
        var open = await opps.CountAsync(o => o.Status == MarketplaceOpportunityStatus.Open, cancellationToken)
            .ConfigureAwait(false);
        var closed = await opps.CountAsync(o => o.Status == MarketplaceOpportunityStatus.Closed, cancellationToken)
            .ConfigureAwait(false);
        var cancelled = await opps.CountAsync(o => o.Status == MarketplaceOpportunityStatus.Cancelled, cancellationToken)
            .ConfigureAwait(false);
        var archived = await opps.CountAsync(o => o.Status == MarketplaceOpportunityStatus.Archived, cancellationToken)
            .ConfigureAwait(false);

        var totalApps = await apps.CountAsync(cancellationToken).ConfigureAwait(false);
        var submitted = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.Submitted, cancellationToken)
            .ConfigureAwait(false);
        var underReview = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.UnderReview, cancellationToken)
            .ConfigureAwait(false);
        var shortlisted = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.Shortlisted, cancellationToken)
            .ConfigureAwait(false);
        var accepted = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.Accepted, cancellationToken)
            .ConfigureAwait(false);
        var rejected = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.Rejected, cancellationToken)
            .ConfigureAwait(false);
        var withdrawn = await apps.CountAsync(a => a.ApplicationStatus == OpportunityApplicationStatus.Withdrawn, cancellationToken)
            .ConfigureAwait(false);

        decimal? avgMatch = null;
        if (await snapshots.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgMatch = await snapshots.AverageAsync(s => s.MatchScore, cancellationToken).ConfigureAwait(false);
        }

        var byType = await opps
            .GroupBy(o => o.OpportunityType)
            .Select(g => new OpportunityTypeBreakdownDto
            {
                OpportunityType = g.Key,
                OpportunityCount = g.Count(),
                OpenCount = g.Count(o => o.Status == MarketplaceOpportunityStatus.Open),
            })
            .OrderBy(x => x.OpportunityType)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<MarketplaceAnalyticsSummaryDto>.Ok(new MarketplaceAnalyticsSummaryDto
        {
            TotalMarketplaceOpportunities = totalOpps,
            DraftOpportunities = draft,
            OpenOpportunities = open,
            ClosedOpportunities = closed,
            CancelledOpportunities = cancelled,
            ArchivedOpportunities = archived,
            TotalApplications = totalApps,
            SubmittedApplications = submitted,
            UnderReviewApplications = underReview,
            ShortlistedApplications = shortlisted,
            AcceptedApplications = accepted,
            RejectedApplications = rejected,
            WithdrawnApplications = withdrawn,
            AverageMatchScore = avgMatch,
            OpportunitiesByType = byType,
        });
    }
}
