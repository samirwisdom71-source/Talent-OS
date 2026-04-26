using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Application.Features.Notifications.Integration;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Services;

public sealed class DevelopmentPlanItemPathService : IDevelopmentPlanItemPathService
{
    private readonly TalentDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IDevelopmentPlanImpactService _impact;

    public DevelopmentPlanItemPathService(
        TalentDbContext db,
        INotificationService notifications,
        IDevelopmentPlanImpactService impact)
    {
        _db = db;
        _notifications = notifications;
        _impact = impact;
    }

    public async Task<Result<DevelopmentPlanItemPathDto>> AddAsync(
        Guid planItemId,
        CreateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == planItemId, cancellationToken);

        if (item is null)
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        if (!PlanAllowsItemMutations(item.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "Paths cannot be added when the parent plan is read-only.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (ItemContentIsReadOnly(item.Status))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "Paths cannot be added to a completed or cancelled item.",
                DevelopmentErrors.DevelopmentPlanItemReadOnly);
        }

        var title = request.Title.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail("Path title is required.");
        }

        var entity = new DevelopmentPlanItemPath
        {
            DevelopmentPlanItemId = planItemId,
            SortOrder = request.SortOrder,
            Title = title,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            PlannedStartUtc = request.PlannedStartUtc,
            PlannedEndUtc = request.PlannedEndUtc,
            Status = DevelopmentItemStatus.NotStarted,
        };

        if (request.Helpers is { Count: > 0 })
        {
            foreach (var h in request.Helpers)
            {
                entity.Helpers.Add(new DevelopmentPlanItemPathHelper
                {
                    HelperKind = h.HelperKind,
                    HelperEntityId = h.HelperEntityId,
                });
            }
        }

        _db.DevelopmentPlanItemPaths.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await RedistributeAchievedImpactsForPlanAsync(item.DevelopmentPlanId, cancellationToken).ConfigureAwait(false);
        await SyncItemProgressFromPathsAsync(planItemId, cancellationToken).ConfigureAwait(false);

        return Result<DevelopmentPlanItemPathDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanItemPathDto>> UpdateAsync(
        Guid pathId,
        UpdateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlanItemPaths
            .Include(x => x.DevelopmentPlanItem)
            .ThenInclude(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == pathId, cancellationToken);

        if (entity is null)
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The development plan item path was not found.",
                DevelopmentErrors.DevelopmentPlanItemPathNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlanItem.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The path cannot be updated when the parent plan is read-only.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (entity.Status is DevelopmentItemStatus.Completed or DevelopmentItemStatus.Cancelled)
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "Completed or cancelled paths cannot be updated.",
                DevelopmentErrors.InvalidItemStatusForOperation);
        }

        var title = request.Title.Trim();
        if (string.IsNullOrEmpty(title))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail("Path title is required.");
        }

        entity.SortOrder = request.SortOrder;
        entity.Title = title;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.PlannedStartUtc = request.PlannedStartUtc;
        entity.PlannedEndUtc = request.PlannedEndUtc;
        entity.Status = request.Status;

        if (request.Helpers is not null)
        {
            var existing = await _db.DevelopmentPlanItemPathHelpers
                .Where(x => x.DevelopmentPlanItemPathId == pathId)
                .ToListAsync(cancellationToken);
            foreach (var h in existing)
            {
                h.RecordStatus = RecordStatus.Deleted;
            }

            foreach (var h in request.Helpers)
            {
                _db.DevelopmentPlanItemPathHelpers.Add(new DevelopmentPlanItemPathHelper
                {
                    DevelopmentPlanItemPathId = pathId,
                    HelperKind = h.HelperKind,
                    HelperEntityId = h.HelperEntityId,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (entity.Status == DevelopmentItemStatus.Completed)
        {
            var planId = entity.DevelopmentPlanItem.DevelopmentPlanId;
            await RedistributeAchievedImpactsForPlanAsync(planId, cancellationToken).ConfigureAwait(false);
        }

        await SyncItemProgressFromPathsAsync(entity.DevelopmentPlanItemId, cancellationToken).ConfigureAwait(false);

        if (entity.Status == DevelopmentItemStatus.Completed)
        {
            await TryCompleteItemAndPlanAsync(entity.DevelopmentPlanItemId, cancellationToken).ConfigureAwait(false);
        }

        return Result<DevelopmentPlanItemPathDto>.Ok(await MapToDtoAsync(pathId, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanItemPathDto>> GetByIdAsync(Guid pathId, CancellationToken cancellationToken = default)
    {
        if (!await _db.DevelopmentPlanItemPaths.AsNoTracking().AnyAsync(x => x.Id == pathId, cancellationToken))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The development plan item path was not found.",
                DevelopmentErrors.DevelopmentPlanItemPathNotFound);
        }

        return Result<DevelopmentPlanItemPathDto>.Ok(await MapToDtoAsync(pathId, cancellationToken));
    }

    public async Task<Result> RemoveAsync(Guid pathId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlanItemPaths
            .Include(x => x.DevelopmentPlanItem)
            .ThenInclude(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == pathId, cancellationToken);

        if (entity is null)
        {
            return Result.Fail(
                "The development plan item path was not found.",
                DevelopmentErrors.DevelopmentPlanItemPathNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlanItem.DevelopmentPlan.Status))
        {
            return Result.Fail(
                "The path cannot be removed when the parent plan is read-only.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        var itemId = entity.DevelopmentPlanItemId;
        var planId = entity.DevelopmentPlanItem.DevelopmentPlanId;
        entity.RecordStatus = RecordStatus.Deleted;
        await _db.SaveChangesAsync(cancellationToken);

        await RedistributeAchievedImpactsForPlanAsync(planId, cancellationToken).ConfigureAwait(false);
        await SyncItemProgressFromPathsAsync(itemId, cancellationToken).ConfigureAwait(false);

        return Result.Ok();
    }

    public async Task<Result<DevelopmentPlanItemPathDto>> MarkCompletedAsync(
        Guid pathId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlanItemPaths
            .Include(x => x.DevelopmentPlanItem)
            .ThenInclude(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == pathId, cancellationToken);

        if (entity is null)
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The development plan item path was not found.",
                DevelopmentErrors.DevelopmentPlanItemPathNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlanItem.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "The path cannot be completed when the parent plan is read-only.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (entity.Status == DevelopmentItemStatus.Completed)
        {
            return Result<DevelopmentPlanItemPathDto>.Ok(await MapToDtoAsync(pathId, cancellationToken));
        }

        if (entity.Status == DevelopmentItemStatus.Cancelled)
        {
            return Result<DevelopmentPlanItemPathDto>.Fail(
                "A cancelled path cannot be marked completed.",
                DevelopmentErrors.InvalidItemStatusForOperation);
        }

        var itemId = entity.DevelopmentPlanItemId;
        var planId = entity.DevelopmentPlanItem.DevelopmentPlanId;
        entity.Status = DevelopmentItemStatus.Completed;
        await _db.SaveChangesAsync(cancellationToken);

        await RedistributeAchievedImpactsForPlanAsync(planId, cancellationToken).ConfigureAwait(false);
        await SyncItemProgressFromPathsAsync(itemId, cancellationToken).ConfigureAwait(false);
        await TryCompleteItemAndPlanAsync(itemId, cancellationToken).ConfigureAwait(false);

        return Result<DevelopmentPlanItemPathDto>.Ok(await MapToDtoAsync(pathId, cancellationToken));
    }

    /// <summary>
    /// يوزّع 100 نقطة أثر على جميع المسارات غير المحذوفة في الخطة؛ كل مسار مكتمل يحصل على نفس الحصة (تُحدَّث عند إضافة/حذف/إكمال مسار).
    /// </summary>
    private async Task RedistributeAchievedImpactsForPlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var paths = await _db.DevelopmentPlanItemPaths
            .Include(p => p.DevelopmentPlanItem)
            .Where(p =>
                p.DevelopmentPlanItem.DevelopmentPlanId == planId &&
                p.RecordStatus != RecordStatus.Deleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var total = paths.Count;
        if (total == 0)
        {
            return;
        }

        var share = Math.Round(100m / total, 4, MidpointRounding.AwayFromZero);

        foreach (var p in paths)
        {
            p.AchievedImpactValue = p.Status == DevelopmentItemStatus.Completed ? share : null;
        }

        var completed = paths.Where(p => p.Status == DevelopmentItemStatus.Completed).ToList();
        if (completed.Count == total)
        {
            var sum = completed.Sum(p => p.AchievedImpactValue ?? 0m);
            var drift = 100m - sum;
            if (drift != 0m)
            {
                var last = completed
                    .OrderByDescending(x => x.ModifiedOnUtc ?? x.CreatedOnUtc)
                    .First();
                last.AchievedImpactValue = (last.AchievedImpactValue ?? 0m) + drift;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SyncItemProgressFromPathsAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var activePaths = await _db.DevelopmentPlanItemPaths.AsNoTracking()
            .Where(x => x.DevelopmentPlanItemId == itemId && x.RecordStatus != RecordStatus.Deleted)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);

        if (activePaths.Count == 0)
        {
            return;
        }

        var item = await _db.DevelopmentPlanItems
            .FirstOrDefaultAsync(x => x.Id == itemId, cancellationToken);

        if (item is null)
        {
            return;
        }

        if (item.Status is DevelopmentItemStatus.Completed or DevelopmentItemStatus.Cancelled)
        {
            return;
        }

        var completed = activePaths.Count(s => s == DevelopmentItemStatus.Completed);
        var pct = Math.Round(100m * completed / activePaths.Count, 2, MidpointRounding.AwayFromZero);

        item.ProgressPercentage = pct;
        if (pct > 0 && pct < 100m && item.Status == DevelopmentItemStatus.NotStarted)
        {
            item.Status = DevelopmentItemStatus.InProgress;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task TryCompleteItemAndPlanAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var paths = await _db.DevelopmentPlanItemPaths
            .AsNoTracking()
            .Where(x => x.DevelopmentPlanItemId == itemId && x.RecordStatus != RecordStatus.Deleted)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);

        if (paths.Count == 0)
        {
            return;
        }

        if (paths.Any(s => s != DevelopmentItemStatus.Completed))
        {
            return;
        }

        var item = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstAsync(x => x.Id == itemId, cancellationToken);

        if (item.Status == DevelopmentItemStatus.Completed)
        {
            await TryAutoCompletePlanAsync(item.DevelopmentPlanId, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (item.Status == DevelopmentItemStatus.Cancelled)
        {
            return;
        }

        item.Status = DevelopmentItemStatus.Completed;
        item.ProgressPercentage = 100m;
        await _db.SaveChangesAsync(cancellationToken);

        await TryAutoCompletePlanAsync(item.DevelopmentPlanId, cancellationToken).ConfigureAwait(false);
    }

    private async Task TryAutoCompletePlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _db.DevelopmentPlans.FirstAsync(x => x.Id == planId, cancellationToken);
        if (plan.Status != DevelopmentPlanStatus.Active)
        {
            return;
        }

        var itemStatuses = await _db.DevelopmentPlanItems
            .AsNoTracking()
            .Where(x => x.DevelopmentPlanId == planId)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);

        if (itemStatuses.Count == 0)
        {
            return;
        }

        if (itemStatuses.Any(s => s != DevelopmentItemStatus.Completed))
        {
            return;
        }

        plan.Status = DevelopmentPlanStatus.Completed;
        await _db.SaveChangesAsync(cancellationToken);

        await _impact.ComputeAndPersistAsync(plan.Id, DevelopmentImpactPhase.After, cancellationToken)
            .ConfigureAwait(false);

        await NotifyPlanCompletedAsync(plan.Id, plan.EmployeeId, plan.PlanTitle, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task NotifyPlanCompletedAsync(
        Guid planId,
        Guid employeeId,
        string planTitle,
        CancellationToken cancellationToken)
    {
        var userId = await _db.Users.AsNoTracking()
            .Where(u => u.EmployeeId == employeeId && u.IsActive)
            .OrderBy(u => u.CreatedOnUtc)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (userId is null)
        {
            return;
        }

        await _notifications.PublishIntegrationAsync(
                DevelopmentNotificationRequests.ForPlanCompleted(planId, userId.Value, planTitle),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<DevelopmentPlanItemPathDto> MapToDtoAsync(Guid pathId, CancellationToken cancellationToken)
    {
        var entity = await _db.DevelopmentPlanItemPaths.AsNoTracking()
            .FirstAsync(x => x.Id == pathId, cancellationToken);

        var helpers = await _db.DevelopmentPlanItemPathHelpers.AsNoTracking()
            .Where(x => x.DevelopmentPlanItemPathId == pathId)
            .OrderBy(x => x.HelperKind)
            .ThenBy(x => x.HelperEntityId)
            .Select(x => new DevelopmentPlanItemPathHelperDto
            {
                Id = x.Id,
                DevelopmentPlanItemPathId = x.DevelopmentPlanItemPathId,
                HelperKind = x.HelperKind,
                HelperEntityId = x.HelperEntityId,
            })
            .ToListAsync(cancellationToken);

        return new DevelopmentPlanItemPathDto
        {
            Id = entity.Id,
            DevelopmentPlanItemId = entity.DevelopmentPlanItemId,
            SortOrder = entity.SortOrder,
            Title = entity.Title,
            Description = entity.Description,
            PlannedStartUtc = entity.PlannedStartUtc,
            PlannedEndUtc = entity.PlannedEndUtc,
            Status = entity.Status,
            AchievedImpactValue = entity.AchievedImpactValue,
            Helpers = helpers,
        };
    }

    private static bool PlanAllowsItemMutations(DevelopmentPlanStatus status) =>
        status is DevelopmentPlanStatus.Draft or DevelopmentPlanStatus.Active;

    private static bool ItemContentIsReadOnly(DevelopmentItemStatus status) =>
        status is DevelopmentItemStatus.Completed or DevelopmentItemStatus.Cancelled;
}
