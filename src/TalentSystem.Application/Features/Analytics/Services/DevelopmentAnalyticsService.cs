using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class DevelopmentAnalyticsService : IDevelopmentAnalyticsService
{
    private readonly TalentDbContext _db;

    public DevelopmentAnalyticsService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DevelopmentAnalyticsSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var plans = _db.DevelopmentPlans.AsNoTracking();
        var items = _db.DevelopmentPlanItems.AsNoTracking();

        var totalPlans = await plans.CountAsync(cancellationToken).ConfigureAwait(false);
        var activePlans = await plans.CountAsync(p => p.Status == DevelopmentPlanStatus.Active, cancellationToken)
            .ConfigureAwait(false);
        var completedPlans = await plans.CountAsync(p => p.Status == DevelopmentPlanStatus.Completed, cancellationToken)
            .ConfigureAwait(false);
        var cancelledPlans = await plans.CountAsync(p => p.Status == DevelopmentPlanStatus.Cancelled, cancellationToken)
            .ConfigureAwait(false);

        var totalItems = await items.CountAsync(cancellationToken).ConfigureAwait(false);
        var completedItems = await items.CountAsync(i => i.Status == DevelopmentItemStatus.Completed, cancellationToken)
            .ConfigureAwait(false);
        var inProgressItems = await items.CountAsync(i => i.Status == DevelopmentItemStatus.InProgress, cancellationToken)
            .ConfigureAwait(false);

        decimal? avgProgress = null;
        var activeItemQuery = items.Where(i =>
            i.Status != DevelopmentItemStatus.Completed &&
            i.Status != DevelopmentItemStatus.Cancelled &&
            i.DevelopmentPlan.Status == DevelopmentPlanStatus.Active);

        if (await activeItemQuery.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            avgProgress = await activeItemQuery.AverageAsync(i => i.ProgressPercentage, cancellationToken)
                .ConfigureAwait(false);
        }

        var byType = await items
            .GroupBy(i => i.ItemType)
            .Select(g => new DevelopmentItemTypeBreakdownDto
            {
                ItemType = g.Key,
                ItemCount = g.Count(),
                CompletedCount = g.Count(i => i.Status == DevelopmentItemStatus.Completed),
                InProgressCount = g.Count(i => i.Status == DevelopmentItemStatus.InProgress)
            })
            .OrderBy(x => x.ItemType)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<DevelopmentAnalyticsSummaryDto>.Ok(new DevelopmentAnalyticsSummaryDto
        {
            TotalDevelopmentPlans = totalPlans,
            ActiveDevelopmentPlans = activePlans,
            CompletedDevelopmentPlans = completedPlans,
            CancelledDevelopmentPlans = cancelledPlans,
            TotalDevelopmentPlanItems = totalItems,
            CompletedDevelopmentPlanItems = completedItems,
            InProgressDevelopmentPlanItems = inProgressItems,
            AverageProgressPercentageActiveItems = avgProgress,
            ItemsByType = byType
        });
    }
}
