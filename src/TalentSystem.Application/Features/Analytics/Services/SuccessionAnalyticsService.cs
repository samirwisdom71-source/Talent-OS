using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class SuccessionAnalyticsService : ISuccessionAnalyticsService
{
    private readonly TalentDbContext _db;

    public SuccessionAnalyticsService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SuccessionAnalyticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // EF Core DbContext does not support concurrent operations; await counts sequentially.
        var totalCritical = await _db.CriticalPositions.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
        var activeCritical = await _db.CriticalPositions.AsNoTracking()
            .Where(cp => _db.SuccessionPlans.Any(p =>
                p.CriticalPositionId == cp.Id && p.Status == SuccessionPlanStatus.Active))
            .CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPlans = await _db.SuccessionPlans.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
        var activePlans = await _db.SuccessionPlans.AsNoTracking()
            .CountAsync(p => p.Status == SuccessionPlanStatus.Active, cancellationToken).ConfigureAwait(false);
        var readyNowPlans = await _db.SuccessionPlans.AsNoTracking()
            .CountAsync(p => p.SuccessorCandidates.Any(c => c.ReadinessLevel == ReadinessLevel.ReadyNow), cancellationToken)
            .ConfigureAwait(false);
        var primaryPlans = await _db.SuccessionPlans.AsNoTracking()
            .CountAsync(p => p.SuccessorCandidates.Any(c => c.IsPrimarySuccessor), cancellationToken).ConfigureAwait(false);

        decimal? avgCoverage = null;
        if (await _db.SuccessionCoverageSnapshots.AsNoTracking().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgCoverage = await _db.SuccessionCoverageSnapshots.AsNoTracking()
                .AverageAsync(s => s.CoverageScore, cancellationToken)
                .ConfigureAwait(false);
        }

        var readiness = await _db.SuccessorCandidates.AsNoTracking()
            .GroupBy(c => c.ReadinessLevel)
            .Select(g => new EnumCountDto<ReadinessLevel> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<SuccessionAnalyticsSummaryDto>.Ok(new SuccessionAnalyticsSummaryDto
        {
            TotalCriticalPositions = totalCritical,
            ActiveCriticalPositions = activeCritical,
            TotalSuccessionPlans = totalPlans,
            ActiveSuccessionPlans = activePlans,
            PlansWithReadyNowSuccessor = readyNowPlans,
            PlansWithPrimarySuccessor = primaryPlans,
            AverageCoverageScore = avgCoverage,
            SuccessorReadinessBreakdown = readiness
        });
    }
}
