using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class PerformanceAnalyticsService : IPerformanceAnalyticsService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<AnalyticsDateRangeFilter> _dateRangeValidator;

    public PerformanceAnalyticsService(
        TalentDbContext db,
        IValidator<AnalyticsDateRangeFilter> dateRangeValidator)
    {
        _db = db;
        _dateRangeValidator = dateRangeValidator;
    }

    public async Task<Result<PerformanceAnalyticsSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var guard = await AnalyticsDateRangeGuard.ValidateAsync(_dateRangeValidator, dateRange, cancellationToken)
            .ConfigureAwait(false);
        if (guard.IsFailure)
        {
            return Result<PerformanceAnalyticsSummaryDto>.Fail(guard.Errors, guard.FailureCode);
        }

        var useRange = dateRange?.FromUtc is not null && dateRange.ToUtc is not null;
        var fromUtc = dateRange?.FromUtc ?? DateTime.MinValue;
        var toUtc = dateRange?.ToUtc ?? DateTime.MaxValue;

        var cycles = _db.PerformanceCycles.AsNoTracking();
        var goals = _db.PerformanceGoals.AsNoTracking();
        if (useRange)
        {
            goals = goals.Where(g => g.CreatedOnUtc >= fromUtc && g.CreatedOnUtc <= toUtc);
        }

        var allEvals = _db.PerformanceEvaluations.AsNoTracking();
        var totalEvals = useRange
            ? await allEvals
                .CountAsync(
                    e => (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                         (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc,
                    cancellationToken).ConfigureAwait(false)
            : await allEvals.CountAsync(cancellationToken).ConfigureAwait(false);

        var finalizedEvalQuery = _db.PerformanceEvaluations.AsNoTracking()
            .Where(e => e.Status == PerformanceEvaluationStatus.Finalized);
        if (useRange)
        {
            finalizedEvalQuery = finalizedEvalQuery.Where(e =>
                (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc);
        }

        var finalizedEvals = await finalizedEvalQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        decimal? avgFinalized = null;
        if (await finalizedEvalQuery.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgFinalized = await finalizedEvalQuery.AverageAsync(e => e.OverallScore, cancellationToken)
                .ConfigureAwait(false);
        }

        var totalCycles = await cycles.CountAsync(cancellationToken).ConfigureAwait(false);
        var activeCycles = await cycles.CountAsync(c => c.Status == PerformanceCycleStatus.Active, cancellationToken)
            .ConfigureAwait(false);

        var totalGoals = await goals.CountAsync(cancellationToken).ConfigureAwait(false);
        var completedGoals = await goals
            .CountAsync(g => g.Status == PerformanceGoalStatus.Completed, cancellationToken).ConfigureAwait(false);

        List<PerformanceCycleAnalyticsBreakdownDto> breakdown;
        if (useRange)
        {
            breakdown = await cycles
                .Select(c => new PerformanceCycleAnalyticsBreakdownDto
                {
                    PerformanceCycleId = c.Id,
                    PerformanceCycleNameEn = c.NameEn,
                    PerformanceCycleNameAr = c.NameAr,
                    TotalGoals = c.Goals.Count(g => g.CreatedOnUtc >= fromUtc && g.CreatedOnUtc <= toUtc),
                    CompletedGoals = c.Goals.Count(g =>
                        g.Status == PerformanceGoalStatus.Completed &&
                        g.CreatedOnUtc >= fromUtc &&
                        g.CreatedOnUtc <= toUtc),
                    TotalEvaluations = c.Evaluations.Count(e =>
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc),
                    FinalizedEvaluations = c.Evaluations.Count(e =>
                        e.Status == PerformanceEvaluationStatus.Finalized &&
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc),
                    AverageFinalizedOverallScore = c.Evaluations
                        .Where(e =>
                            e.Status == PerformanceEvaluationStatus.Finalized &&
                            (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                            (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc)
                        .Select(e => (decimal?)e.OverallScore)
                        .Average(),
                })
                .OrderByDescending(x => x.TotalEvaluations)
                .ThenBy(x => x.PerformanceCycleNameEn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            breakdown = await cycles
                .Select(c => new PerformanceCycleAnalyticsBreakdownDto
                {
                    PerformanceCycleId = c.Id,
                    PerformanceCycleNameEn = c.NameEn,
                    PerformanceCycleNameAr = c.NameAr,
                    TotalGoals = c.Goals.Count(),
                    CompletedGoals = c.Goals.Count(g => g.Status == PerformanceGoalStatus.Completed),
                    TotalEvaluations = c.Evaluations.Count(),
                    FinalizedEvaluations = c.Evaluations.Count(e => e.Status == PerformanceEvaluationStatus.Finalized),
                    AverageFinalizedOverallScore = c.Evaluations
                        .Where(e => e.Status == PerformanceEvaluationStatus.Finalized)
                        .Select(e => (decimal?)e.OverallScore)
                        .Average(),
                })
                .OrderByDescending(x => x.TotalEvaluations)
                .ThenBy(x => x.PerformanceCycleNameEn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return Result<PerformanceAnalyticsSummaryDto>.Ok(new PerformanceAnalyticsSummaryDto
        {
            TotalPerformanceCycles = totalCycles,
            ActivePerformanceCycles = activeCycles,
            TotalGoals = totalGoals,
            CompletedGoals = completedGoals,
            TotalEvaluations = totalEvals,
            FinalizedEvaluations = finalizedEvals,
            AverageOverallEvaluationScoreFinalized = avgFinalized,
            BreakdownByCycle = breakdown,
        });
    }

    public async Task<Result<PerformanceImpactSummaryDto>> GetImpactAsync(
        PerformanceImpactFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var afterTo = request.AfterToUtc ?? now;
        var afterFrom = request.AfterFromUtc ?? afterTo.AddDays(-90);
        var beforeTo = request.BeforeToUtc ?? afterFrom.AddDays(-1);
        var beforeFrom = request.BeforeFromUtc ?? beforeTo.AddDays(-90);

        if (beforeFrom > beforeTo || afterFrom > afterTo)
        {
            return Result<PerformanceImpactSummaryDto>.Fail("Invalid date range for period comparison.");
        }

        var evals = _db.PerformanceEvaluations.AsNoTracking();
        var goals = _db.PerformanceGoals.AsNoTracking();
        var developmentPlans = _db.DevelopmentPlans.AsNoTracking();
        var successionPlans = _db.SuccessionPlans.AsNoTracking();
        var marketplaceApplications = _db.OpportunityApplications.AsNoTracking();
        var talentClassifications = _db.TalentClassifications.AsNoTracking();

        var beforeFinalized = evals
            .Where(e => e.Status == PerformanceEvaluationStatus.Finalized &&
                        e.CreatedOnUtc >= beforeFrom &&
                        e.CreatedOnUtc <= beforeTo);

        var afterFinalized = evals
            .Where(e => e.Status == PerformanceEvaluationStatus.Finalized &&
                        e.CreatedOnUtc >= afterFrom &&
                        e.CreatedOnUtc <= afterTo);

        var beforeFinalizedCount = await beforeFinalized.CountAsync(cancellationToken).ConfigureAwait(false);
        var afterFinalizedCount = await afterFinalized.CountAsync(cancellationToken).ConfigureAwait(false);

        decimal? beforeAvg = null;
        if (beforeFinalizedCount > 0)
        {
            beforeAvg = await beforeFinalized.AverageAsync(e => e.OverallScore, cancellationToken).ConfigureAwait(false);
        }

        decimal? afterAvg = null;
        if (afterFinalizedCount > 0)
        {
            afterAvg = await afterFinalized.AverageAsync(e => e.OverallScore, cancellationToken).ConfigureAwait(false);
        }

        var beforeGoalTotal = await goals
            .CountAsync(g => g.CreatedOnUtc >= beforeFrom && g.CreatedOnUtc <= beforeTo, cancellationToken)
            .ConfigureAwait(false);
        var beforeGoalDone = await goals
            .CountAsync(g => g.CreatedOnUtc >= beforeFrom && g.CreatedOnUtc <= beforeTo && g.Status == PerformanceGoalStatus.Completed, cancellationToken)
            .ConfigureAwait(false);

        var afterGoalTotal = await goals
            .CountAsync(g => g.CreatedOnUtc >= afterFrom && g.CreatedOnUtc <= afterTo, cancellationToken)
            .ConfigureAwait(false);
        var afterGoalDone = await goals
            .CountAsync(g => g.CreatedOnUtc >= afterFrom && g.CreatedOnUtc <= afterTo && g.Status == PerformanceGoalStatus.Completed, cancellationToken)
            .ConfigureAwait(false);

        var beforeCompletion = beforeGoalTotal == 0 ? 0m : Math.Round((decimal)beforeGoalDone / beforeGoalTotal * 100m, 2);
        var afterCompletion = afterGoalTotal == 0 ? 0m : Math.Round((decimal)afterGoalDone / afterGoalTotal * 100m, 2);

        var beforeDevPlans = await developmentPlans
            .CountAsync(p => p.CreatedOnUtc >= beforeFrom && p.CreatedOnUtc <= beforeTo, cancellationToken)
            .ConfigureAwait(false);
        var afterDevPlans = await developmentPlans
            .CountAsync(p => p.CreatedOnUtc >= afterFrom && p.CreatedOnUtc <= afterTo, cancellationToken)
            .ConfigureAwait(false);

        var beforeSuccPlans = await successionPlans
            .CountAsync(p => p.CreatedOnUtc >= beforeFrom && p.CreatedOnUtc <= beforeTo, cancellationToken)
            .ConfigureAwait(false);
        var afterSuccPlans = await successionPlans
            .CountAsync(p => p.CreatedOnUtc >= afterFrom && p.CreatedOnUtc <= afterTo, cancellationToken)
            .ConfigureAwait(false);

        var beforeApps = await marketplaceApplications
            .CountAsync(a => a.AppliedOnUtc >= beforeFrom && a.AppliedOnUtc <= beforeTo, cancellationToken)
            .ConfigureAwait(false);
        var afterApps = await marketplaceApplications
            .CountAsync(a => a.AppliedOnUtc >= afterFrom && a.AppliedOnUtc <= afterTo, cancellationToken)
            .ConfigureAwait(false);

        var beforeClassifications = await talentClassifications
            .CountAsync(t => t.ClassifiedOnUtc >= beforeFrom && t.ClassifiedOnUtc <= beforeTo, cancellationToken)
            .ConfigureAwait(false);
        var afterClassifications = await talentClassifications
            .CountAsync(t => t.ClassifiedOnUtc >= afterFrom && t.ClassifiedOnUtc <= afterTo, cancellationToken)
            .ConfigureAwait(false);

        return Result<PerformanceImpactSummaryDto>.Ok(new PerformanceImpactSummaryDto
        {
            BeforeFromUtc = beforeFrom,
            BeforeToUtc = beforeTo,
            AfterFromUtc = afterFrom,
            AfterToUtc = afterTo,
            BeforeFinalizedEvaluations = beforeFinalizedCount,
            AfterFinalizedEvaluations = afterFinalizedCount,
            BeforeAverageScore = beforeAvg,
            AfterAverageScore = afterAvg,
            ScoreDelta = (afterAvg.HasValue && beforeAvg.HasValue) ? Math.Round(afterAvg.Value - beforeAvg.Value, 2) : null,
            BeforeGoalCompletionRate = beforeCompletion,
            AfterGoalCompletionRate = afterCompletion,
            GoalCompletionRateDelta = Math.Round(afterCompletion - beforeCompletion, 2),
            BeforeDevelopmentPlansCreated = beforeDevPlans,
            AfterDevelopmentPlansCreated = afterDevPlans,
            BeforeSuccessionPlansCreated = beforeSuccPlans,
            AfterSuccessionPlansCreated = afterSuccPlans,
            BeforeMarketplaceApplications = beforeApps,
            AfterMarketplaceApplications = afterApps,
            BeforeTalentClassifications = beforeClassifications,
            AfterTalentClassifications = afterClassifications,
        });
    }
}
