using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class PerformanceAnalyticsService : IPerformanceAnalyticsService
{
    private readonly TalentDbContext _db;

    public PerformanceAnalyticsService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PerformanceAnalyticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cycles = _db.PerformanceCycles.AsNoTracking();
        var goals = _db.PerformanceGoals.AsNoTracking();
        var evals = _db.PerformanceEvaluations.AsNoTracking();

        var totalCycles = await cycles.CountAsync(cancellationToken).ConfigureAwait(false);
        var activeCycles = await cycles.CountAsync(c => c.Status == PerformanceCycleStatus.Active, cancellationToken)
            .ConfigureAwait(false);

        var totalGoals = await goals.CountAsync(cancellationToken).ConfigureAwait(false);
        var completedGoals = await goals.CountAsync(g => g.Status == PerformanceGoalStatus.Completed, cancellationToken)
            .ConfigureAwait(false);

        var totalEvals = await evals.CountAsync(cancellationToken).ConfigureAwait(false);
        var finalizedEvals = await evals.CountAsync(e => e.Status == PerformanceEvaluationStatus.Finalized, cancellationToken)
            .ConfigureAwait(false);

        decimal? avgFinalized = null;
        var finalizedQuery = evals.Where(e => e.Status == PerformanceEvaluationStatus.Finalized);
        if (await finalizedQuery.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgFinalized = await finalizedQuery.AverageAsync(e => e.OverallScore, cancellationToken)
                .ConfigureAwait(false);
        }

        var breakdown = await cycles
            .Select(c => new PerformanceCycleAnalyticsBreakdownDto
            {
                PerformanceCycleId = c.Id,
                PerformanceCycleNameEn = c.NameEn,
                TotalGoals = c.Goals.Count(),
                CompletedGoals = c.Goals.Count(g => g.Status == PerformanceGoalStatus.Completed),
                TotalEvaluations = c.Evaluations.Count(),
                FinalizedEvaluations = c.Evaluations.Count(e => e.Status == PerformanceEvaluationStatus.Finalized),
                AverageFinalizedOverallScore = c.Evaluations
                    .Where(e => e.Status == PerformanceEvaluationStatus.Finalized)
                    .Select(e => (decimal?)e.OverallScore)
                    .Average()
            })
            .OrderByDescending(x => x.TotalEvaluations)
            .ThenBy(x => x.PerformanceCycleNameEn)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PerformanceAnalyticsSummaryDto>.Ok(new PerformanceAnalyticsSummaryDto
        {
            TotalPerformanceCycles = totalCycles,
            ActivePerformanceCycles = activeCycles,
            TotalGoals = totalGoals,
            CompletedGoals = completedGoals,
            TotalEvaluations = totalEvals,
            FinalizedEvaluations = finalizedEvals,
            AverageOverallEvaluationScoreFinalized = avgFinalized,
            BreakdownByCycle = breakdown
        });
    }
}
