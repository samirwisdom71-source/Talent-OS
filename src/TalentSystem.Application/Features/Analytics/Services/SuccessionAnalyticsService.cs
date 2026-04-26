using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class SuccessionAnalyticsService : ISuccessionAnalyticsService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<AnalyticsDateRangeFilter> _dateRangeValidator;

    public SuccessionAnalyticsService(
        TalentDbContext db,
        IValidator<AnalyticsDateRangeFilter> dateRangeValidator)
    {
        _db = db;
        _dateRangeValidator = dateRangeValidator;
    }

    public async Task<Result<SuccessionAnalyticsSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var guard = await AnalyticsDateRangeGuard.ValidateAsync(_dateRangeValidator, dateRange, cancellationToken)
            .ConfigureAwait(false);
        if (guard.IsFailure)
        {
            return Result<SuccessionAnalyticsSummaryDto>.Fail(guard.Errors, guard.FailureCode);
        }

        var useRange = dateRange?.FromUtc is not null && dateRange.ToUtc is not null;
        var fromUtc = dateRange?.FromUtc ?? DateTime.MinValue;
        var toUtc = dateRange?.ToUtc ?? DateTime.MaxValue;

        var critical = _db.CriticalPositions.AsNoTracking();
        var successionPlans = _db.SuccessionPlans.AsNoTracking();
        var candidates = _db.SuccessorCandidates.AsNoTracking();
        var snapshots = _db.SuccessionCoverageSnapshots.AsNoTracking();

        if (useRange)
        {
            critical = critical.Where(c => c.CreatedOnUtc >= fromUtc && c.CreatedOnUtc <= toUtc);
            successionPlans = successionPlans.Where(p => p.CreatedOnUtc >= fromUtc && p.CreatedOnUtc <= toUtc);
            candidates = candidates.Where(c => c.CreatedOnUtc >= fromUtc && c.CreatedOnUtc <= toUtc);
            snapshots = snapshots.Where(s => s.CreatedOnUtc >= fromUtc && s.CreatedOnUtc <= toUtc);
        }

        var totalCritical = await critical.CountAsync(cancellationToken).ConfigureAwait(false);
        var activeCritical = await critical
            .Where(cp => _db.SuccessionPlans.Any(p =>
                p.CriticalPositionId == cp.Id && p.Status == SuccessionPlanStatus.Active))
            .CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPlans = await successionPlans.CountAsync(cancellationToken).ConfigureAwait(false);
        var activePlans = await successionPlans
            .CountAsync(p => p.Status == SuccessionPlanStatus.Active, cancellationToken).ConfigureAwait(false);
        var readyNowPlans = await successionPlans
            .CountAsync(p => p.SuccessorCandidates.Any(c => c.ReadinessLevel == ReadinessLevel.ReadyNow), cancellationToken)
            .ConfigureAwait(false);
        var primaryPlans = await successionPlans
            .CountAsync(p => p.SuccessorCandidates.Any(c => c.IsPrimarySuccessor), cancellationToken).ConfigureAwait(false);

        decimal? avgCoverage = null;
        if (await snapshots.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgCoverage = await snapshots.AverageAsync(s => s.CoverageScore, cancellationToken).ConfigureAwait(false);
        }

        var readiness = await candidates
            .GroupBy(c => c.ReadinessLevel)
            .Select(g => new EnumCountDto<ReadinessLevel> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return Result<SuccessionAnalyticsSummaryDto>.Ok(new SuccessionAnalyticsSummaryDto
        {
            TotalCriticalPositions = totalCritical,
            ActiveCriticalPositions = activeCritical,
            TotalSuccessionPlans = totalPlans,
            ActiveSuccessionPlans = activePlans,
            PlansWithReadyNowSuccessor = readyNowPlans,
            PlansWithPrimarySuccessor = primaryPlans,
            AverageCoverageScore = avgCoverage,
            SuccessorReadinessBreakdown = readiness,
        });
    }
}
