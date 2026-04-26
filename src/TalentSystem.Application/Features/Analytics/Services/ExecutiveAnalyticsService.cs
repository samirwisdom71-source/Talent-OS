using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class ExecutiveAnalyticsService : IExecutiveAnalyticsService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<AnalyticsDateRangeFilter> _dateRangeValidator;

    public ExecutiveAnalyticsService(
        TalentDbContext db,
        IValidator<AnalyticsDateRangeFilter> dateRangeValidator)
    {
        _db = db;
        _dateRangeValidator = dateRangeValidator;
    }

    public async Task<Result<ExecutiveDashboardSummaryDto>> GetSummaryAsync(
        AnalyticsDateRangeFilter? dateRange = null,
        CancellationToken cancellationToken = default)
    {
        var guard = await AnalyticsDateRangeGuard.ValidateAsync(_dateRangeValidator, dateRange, cancellationToken)
            .ConfigureAwait(false);
        if (guard.IsFailure)
        {
            return Result<ExecutiveDashboardSummaryDto>.Fail(guard.Errors, guard.FailureCode);
        }

        var useRange = dateRange?.FromUtc is not null && dateRange.ToUtc is not null;
        var fromUtc = dateRange?.FromUtc ?? DateTime.MinValue;
        var toUtc = dateRange?.ToUtc ?? DateTime.MaxValue;

        var tc = _db.TalentClassifications.AsNoTracking();
        if (useRange)
        {
            tc = tc.Where(t => t.ClassifiedOnUtc >= fromUtc && t.ClassifiedOnUtc <= toUtc);
        }

        var goals = _db.PerformanceGoals.AsNoTracking();
        if (useRange)
        {
            goals = goals.Where(g => g.CreatedOnUtc >= fromUtc && g.CreatedOnUtc <= toUtc);
        }

        var finalizedEvals = _db.PerformanceEvaluations.AsNoTracking()
            .Where(e => e.Status == PerformanceEvaluationStatus.Finalized);
        if (useRange)
        {
            finalizedEvals = finalizedEvals.Where(e =>
                (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc);
        }

        var devItems = _db.DevelopmentPlanItems.AsNoTracking();
        if (useRange)
        {
            devItems = devItems.Where(i => i.CreatedOnUtc >= fromUtc && i.CreatedOnUtc <= toUtc);
        }

        var opps = _db.MarketplaceOpportunities.AsNoTracking();
        if (useRange)
        {
            opps = opps.Where(o => o.CreatedOnUtc >= fromUtc && o.CreatedOnUtc <= toUtc);
        }

        var apps = _db.OpportunityApplications.AsNoTracking();
        if (useRange)
        {
            apps = apps.Where(a => a.AppliedOnUtc >= fromUtc && a.AppliedOnUtc <= toUtc);
        }

        var successors = _db.SuccessorCandidates.AsNoTracking();
        if (useRange)
        {
            successors = successors.Where(s => s.CreatedOnUtc >= fromUtc && s.CreatedOnUtc <= toUtc);
        }

        // EF Core: single DbContext — run queries sequentially.
        var totalEmployees = await _db.Employees.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
        var totalTalentScores = await _db.TalentScores.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
        var totalTalentClassifications = await tc.CountAsync(cancellationToken).ConfigureAwait(false);
        var highPotentialCount = await tc.CountAsync(t => t.IsHighPotential, cancellationToken).ConfigureAwait(false);
        var highPerformerCount = await tc.CountAsync(t => t.IsHighPerformer, cancellationToken).ConfigureAwait(false);
        var strategicLeaderCount = await tc.CountAsync(t => t.NineBoxCode == NineBoxCode.Box9, cancellationToken)
            .ConfigureAwait(false);
        var activePerformanceCycleCount = await _db.PerformanceCycles.AsNoTracking()
            .CountAsync(c => c.Status == PerformanceCycleStatus.Active, cancellationToken).ConfigureAwait(false);
        var activeSuccessionPlanCount = await _db.SuccessionPlans.AsNoTracking()
            .CountAsync(p => p.Status == SuccessionPlanStatus.Active, cancellationToken).ConfigureAwait(false);
        var openMarketplaceOpportunityCount = await _db.MarketplaceOpportunities.AsNoTracking()
            .CountAsync(o => o.Status == MarketplaceOpportunityStatus.Open, cancellationToken).ConfigureAwait(false);
        var activeDevelopmentPlanCount = await _db.DevelopmentPlans.AsNoTracking()
            .CountAsync(p => p.Status == DevelopmentPlanStatus.Active, cancellationToken).ConfigureAwait(false);

        var totalPerformanceGoals = await goals.CountAsync(cancellationToken).ConfigureAwait(false);
        var completedGoalsQ = _db.PerformanceGoals.AsNoTracking()
            .Where(g => g.Status == PerformanceGoalStatus.Completed);
        if (useRange)
        {
            completedGoalsQ = completedGoalsQ.Where(g =>
                (g.ModifiedOnUtc ?? g.CreatedOnUtc) >= fromUtc &&
                (g.ModifiedOnUtc ?? g.CreatedOnUtc) <= toUtc);
        }

        var completedPerformanceGoals = await completedGoalsQ.CountAsync(cancellationToken).ConfigureAwait(false);

        var finalizedEvaluationCount = await finalizedEvals.CountAsync(cancellationToken).ConfigureAwait(false);

        var approvals = _db.ApprovalRequests.AsNoTracking();
        if (useRange)
        {
            approvals = approvals.Where(a => a.CreatedOnUtc >= fromUtc && a.CreatedOnUtc <= toUtc);
        }

        var pendingApprovalCount = await approvals
            .CountAsync(
                a => a.Status == ApprovalRequestStatus.Submitted || a.Status == ApprovalRequestStatus.InReview,
                cancellationToken).ConfigureAwait(false);

        var insights = _db.TalentInsights.AsNoTracking();
        var recommendations = _db.TalentRecommendations.AsNoTracking();
        if (useRange)
        {
            insights = insights.Where(i => i.CreatedOnUtc >= fromUtc && i.CreatedOnUtc <= toUtc);
            recommendations = recommendations.Where(r => r.CreatedOnUtc >= fromUtc && r.CreatedOnUtc <= toUtc);
        }

        var talentInsightCount = await insights.CountAsync(cancellationToken).ConfigureAwait(false);
        var talentRecommendationCount = await recommendations.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalMarketplaceApplicationCount = await apps.CountAsync(cancellationToken).ConfigureAwait(false);

        var nineBoxDistribution = await tc
            .GroupBy(t => t.NineBoxCode)
            .Select(g => new NineBoxDistributionItemDto { NineBoxCode = g.Key, Count = g.Count() })
            .OrderBy(x => x.NineBoxCode)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var byPerformanceBand = await tc
            .GroupBy(t => t.PerformanceBand)
            .Select(g => new EnumCountDto<PerformanceBand> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var byPotentialBand = await tc
            .GroupBy(t => t.PotentialBand)
            .Select(g => new EnumCountDto<PotentialBand> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var topTalentCategories = await tc
            .Where(t => t.CategoryName != null && t.CategoryName != string.Empty)
            .GroupBy(t => t.CategoryName)
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var cycleQuery = _db.PerformanceCycles.AsNoTracking();
        List<ExecutiveCycleSnapshotDto> performanceByCycle;
        if (useRange)
        {
            performanceByCycle = await cycleQuery
                .Select(c => new ExecutiveCycleSnapshotDto
                {
                    PerformanceCycleId = c.Id,
                    PerformanceCycleNameEn = c.NameEn,
                    PerformanceCycleNameAr = c.NameAr,
                    FinalizedEvaluations = c.Evaluations.Count(e =>
                        e.Status == PerformanceEvaluationStatus.Finalized &&
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) >= fromUtc &&
                        (e.EvaluatedOnUtc ?? e.CreatedOnUtc) <= toUtc),
                })
                .OrderByDescending(x => x.FinalizedEvaluations)
                .ThenBy(x => x.PerformanceCycleNameEn)
                .Take(16)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            performanceByCycle = await cycleQuery
                .Select(c => new ExecutiveCycleSnapshotDto
                {
                    PerformanceCycleId = c.Id,
                    PerformanceCycleNameEn = c.NameEn,
                    PerformanceCycleNameAr = c.NameAr,
                    FinalizedEvaluations = c.Evaluations.Count(e => e.Status == PerformanceEvaluationStatus.Finalized),
                })
                .OrderByDescending(x => x.FinalizedEvaluations)
                .ThenBy(x => x.PerformanceCycleNameEn)
                .Take(16)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        var developmentItemsByType = await devItems
            .GroupBy(i => i.ItemType)
            .Select(g => new ExecutiveCodeCountDto { Code = (int)g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var marketplaceOpportunitiesByType = await opps
            .GroupBy(o => o.OpportunityType)
            .Select(g => new ExecutiveCodeCountDto { Code = (int)g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var successorReadiness = await successors
            .GroupBy(s => s.ReadinessLevel)
            .Select(g => new EnumCountDto<ReadinessLevel> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var dto = new ExecutiveDashboardSummaryDto
        {
            TotalEmployees = totalEmployees,
            TotalTalentScores = totalTalentScores,
            TotalTalentClassifications = totalTalentClassifications,
            HighPotentialCount = highPotentialCount,
            HighPerformerCount = highPerformerCount,
            StrategicLeaderCount = strategicLeaderCount,
            ActivePerformanceCycleCount = activePerformanceCycleCount,
            ActiveSuccessionPlanCount = activeSuccessionPlanCount,
            OpenMarketplaceOpportunityCount = openMarketplaceOpportunityCount,
            ActiveDevelopmentPlanCount = activeDevelopmentPlanCount,
            TotalPerformanceGoals = totalPerformanceGoals,
            CompletedPerformanceGoals = completedPerformanceGoals,
            FinalizedEvaluationCount = finalizedEvaluationCount,
            PendingApprovalCount = pendingApprovalCount,
            TalentInsightCount = talentInsightCount,
            TalentRecommendationCount = talentRecommendationCount,
            TotalMarketplaceApplicationCount = totalMarketplaceApplicationCount,
            NineBoxDistribution = nineBoxDistribution,
            ByPerformanceBand = byPerformanceBand,
            ByPotentialBand = byPotentialBand,
            TopTalentCategories = topTalentCategories,
            PerformanceByCycle = performanceByCycle,
            DevelopmentItemsByType = developmentItemsByType,
            MarketplaceOpportunitiesByType = marketplaceOpportunitiesByType,
            SuccessorReadiness = successorReadiness,
        };

        return Result<ExecutiveDashboardSummaryDto>.Ok(dto);
    }
}
