using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Services;

public sealed class DevelopmentPlanItemService : IDevelopmentPlanItemService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateDevelopmentPlanItemRequest> _createValidator;
    private readonly IValidator<UpdateDevelopmentPlanItemRequest> _updateValidator;
    private readonly IValidator<DevelopmentPlanItemFilterRequest> _filterValidator;
    private readonly IValidator<UpdateDevelopmentPlanItemProgressRequest> _progressValidator;

    public DevelopmentPlanItemService(
        TalentDbContext db,
        IValidator<CreateDevelopmentPlanItemRequest> createValidator,
        IValidator<UpdateDevelopmentPlanItemRequest> updateValidator,
        IValidator<DevelopmentPlanItemFilterRequest> filterValidator,
        IValidator<UpdateDevelopmentPlanItemProgressRequest> progressValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _progressValidator = progressValidator;
    }

    public async Task<Result<DevelopmentPlanItemDto>> AddAsync(
        CreateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanItemDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var plan = await _db.DevelopmentPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DevelopmentPlanId, cancellationToken);

        if (plan is null)
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        if (!PlanAllowsItemMutations(plan.Status))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Development plan items cannot be added when the plan is completed, cancelled, or archived.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (request.RelatedCompetencyId is { } competencyId &&
            !await _db.Competencies.AsNoTracking().AnyAsync(x => x.Id == competencyId, cancellationToken))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.CompetencyNotFound);
        }

        var entity = new DevelopmentPlanItem
        {
            DevelopmentPlanId = request.DevelopmentPlanId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ItemType = request.ItemType,
            RelatedCompetencyId = request.RelatedCompetencyId,
            TargetDate = request.TargetDate,
            Status = request.Status,
            ProgressPercentage = request.ProgressPercentage,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.DevelopmentPlanItems.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanItemDto>> UpdateAsync(
        Guid id,
        UpdateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanItemDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Development plan items cannot be updated when the parent plan is completed, cancelled, or archived.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (ItemContentIsReadOnly(entity.Status))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Completed or cancelled items cannot be updated.",
                DevelopmentErrors.DevelopmentPlanItemReadOnly);
        }

        if (request.RelatedCompetencyId is { } competencyId &&
            !await _db.Competencies.AsNoTracking().AnyAsync(x => x.Id == competencyId, cancellationToken))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.CompetencyNotFound);
        }

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.ItemType = request.ItemType;
        entity.RelatedCompetencyId = request.RelatedCompetencyId;
        entity.TargetDate = request.TargetDate;
        entity.Status = request.Status;
        entity.ProgressPercentage = request.ProgressPercentage;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanItemDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _db.DevelopmentPlanItems.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(id, cancellationToken));
    }

    public async Task<Result<PagedResult<DevelopmentPlanItemDto>>> GetPagedAsync(
        DevelopmentPlanItemFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<DevelopmentPlanItemDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<DevelopmentPlanItem> query = _db.DevelopmentPlanItems.AsNoTracking()
            .Where(x => x.DevelopmentPlanId == request.DevelopmentPlanId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Title)
            .ThenBy(x => x.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DevelopmentPlanItemDto
            {
                Id = x.Id,
                DevelopmentPlanId = x.DevelopmentPlanId,
                Title = x.Title,
                Description = x.Description,
                ItemType = x.ItemType,
                RelatedCompetencyId = x.RelatedCompetencyId,
                TargetDate = x.TargetDate,
                Status = x.Status,
                ProgressPercentage = x.ProgressPercentage,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);

        await HydratePathsForItemsAsync(items, cancellationToken);

        return Result<PagedResult<DevelopmentPlanItemDto>>.Ok(new PagedResult<DevelopmentPlanItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlan.Status))
        {
            return Result.Fail(
                "Development plan items cannot be removed when the parent plan is completed, cancelled, or archived.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        entity.RecordStatus = RecordStatus.Deleted;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<DevelopmentPlanItemDto>> UpdateProgressAsync(
        Guid id,
        UpdateDevelopmentPlanItemProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _progressValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanItemDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Progress cannot be updated when the parent plan is completed, cancelled, or archived.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (entity.Status is not (DevelopmentItemStatus.NotStarted or DevelopmentItemStatus.InProgress))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Progress can only be updated for items that are not started or in progress.",
                DevelopmentErrors.InvalidItemStatusForOperation);
        }

        entity.ProgressPercentage = request.ProgressPercentage;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanItemDto>> MarkCompletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlanItems
            .Include(x => x.DevelopmentPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "The development plan item was not found.",
                DevelopmentErrors.DevelopmentPlanItemNotFound);
        }

        if (!PlanAllowsItemMutations(entity.DevelopmentPlan.Status))
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "Items cannot be completed when the parent plan is completed, cancelled, or archived.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        if (entity.Status == DevelopmentItemStatus.Completed)
        {
            return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
        }

        if (entity.Status == DevelopmentItemStatus.Cancelled)
        {
            return Result<DevelopmentPlanItemDto>.Fail(
                "A cancelled item cannot be marked completed.",
                DevelopmentErrors.InvalidItemStatusForOperation);
        }

        var hasPaths = await _db.DevelopmentPlanItemPaths.AsNoTracking()
            .AnyAsync(
                x => x.DevelopmentPlanItemId == entity.Id && x.RecordStatus != RecordStatus.Deleted,
                cancellationToken);
        if (hasPaths)
        {
            var allPathsCompleted = await _db.DevelopmentPlanItemPaths.AsNoTracking()
                .Where(x => x.DevelopmentPlanItemId == entity.Id && x.RecordStatus != RecordStatus.Deleted)
                .AllAsync(x => x.Status == DevelopmentItemStatus.Completed, cancellationToken);
            if (!allPathsCompleted)
            {
                return Result<DevelopmentPlanItemDto>.Fail(
                    "Complete all item paths before marking the item completed.",
                    DevelopmentErrors.ItemPathsIncomplete);
            }
        }

        entity.Status = DevelopmentItemStatus.Completed;
        entity.ProgressPercentage = 100m;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanItemDto>.Ok(await MapToDtoAsync(entity.Id, cancellationToken));
    }

    private static bool PlanAllowsItemMutations(DevelopmentPlanStatus status) =>
        status is DevelopmentPlanStatus.Draft or DevelopmentPlanStatus.Active;

    private static bool ItemContentIsReadOnly(DevelopmentItemStatus status) =>
        status is DevelopmentItemStatus.Completed or DevelopmentItemStatus.Cancelled;

    private async Task HydratePathsForItemsAsync(
        List<DevelopmentPlanItemDto> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var itemIds = items.Select(x => x.Id).ToList();
        var pathRows = await _db.DevelopmentPlanItemPaths.AsNoTracking()
            .Where(p => itemIds.Contains(p.DevelopmentPlanItemId) && p.RecordStatus != RecordStatus.Deleted)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        if (pathRows.Count == 0)
        {
            return;
        }

        var pathIds = pathRows.Select(p => p.Id).ToList();
        var helperRows = await _db.DevelopmentPlanItemPathHelpers.AsNoTracking()
            .Where(h => pathIds.Contains(h.DevelopmentPlanItemPathId))
            .OrderBy(h => h.HelperKind)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            var forItem = pathRows.Where(p => p.DevelopmentPlanItemId == item.Id).ToList();
            if (forItem.Count == 0)
            {
                continue;
            }

            item.Paths = forItem.Select(p => new DevelopmentPlanItemPathDto
            {
                Id = p.Id,
                DevelopmentPlanItemId = p.DevelopmentPlanItemId,
                SortOrder = p.SortOrder,
                Title = p.Title,
                Description = p.Description,
                PlannedStartUtc = p.PlannedStartUtc,
                PlannedEndUtc = p.PlannedEndUtc,
                Status = p.Status,
                AchievedImpactValue = p.AchievedImpactValue,
                Helpers = helperRows
                    .Where(h => h.DevelopmentPlanItemPathId == p.Id)
                    .Select(h => new DevelopmentPlanItemPathHelperDto
                    {
                        Id = h.Id,
                        DevelopmentPlanItemPathId = h.DevelopmentPlanItemPathId,
                        HelperKind = h.HelperKind,
                        HelperEntityId = h.HelperEntityId,
                    })
                    .ToList(),
            }).ToList();
        }
    }

    private async Task<DevelopmentPlanItemDto> MapToDtoAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var entity = await _db.DevelopmentPlanItems.AsNoTracking()
            .FirstAsync(x => x.Id == itemId, cancellationToken);

        var pathRows = await _db.DevelopmentPlanItemPaths.AsNoTracking()
            .Where(x => x.DevelopmentPlanItemId == itemId && x.RecordStatus != RecordStatus.Deleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        IReadOnlyList<DevelopmentPlanItemPathDto> paths = Array.Empty<DevelopmentPlanItemPathDto>();
        if (pathRows.Count > 0)
        {
            var pathIds = pathRows.Select(p => p.Id).ToList();
            var helperRows = await _db.DevelopmentPlanItemPathHelpers.AsNoTracking()
                .Where(x => pathIds.Contains(x.DevelopmentPlanItemPathId))
                .OrderBy(x => x.HelperKind)
                .ToListAsync(cancellationToken);

            paths = pathRows.Select(p => new DevelopmentPlanItemPathDto
            {
                Id = p.Id,
                DevelopmentPlanItemId = p.DevelopmentPlanItemId,
                SortOrder = p.SortOrder,
                Title = p.Title,
                Description = p.Description,
                PlannedStartUtc = p.PlannedStartUtc,
                PlannedEndUtc = p.PlannedEndUtc,
                Status = p.Status,
                AchievedImpactValue = p.AchievedImpactValue,
                Helpers = helperRows
                    .Where(h => h.DevelopmentPlanItemPathId == p.Id)
                    .Select(h => new DevelopmentPlanItemPathHelperDto
                    {
                        Id = h.Id,
                        DevelopmentPlanItemPathId = h.DevelopmentPlanItemPathId,
                        HelperKind = h.HelperKind,
                        HelperEntityId = h.HelperEntityId,
                    })
                    .ToList(),
            }).ToList();
        }

        return new DevelopmentPlanItemDto
        {
            Id = entity.Id,
            DevelopmentPlanId = entity.DevelopmentPlanId,
            Title = entity.Title,
            Description = entity.Description,
            ItemType = entity.ItemType,
            RelatedCompetencyId = entity.RelatedCompetencyId,
            TargetDate = entity.TargetDate,
            Status = entity.Status,
            ProgressPercentage = entity.ProgressPercentage,
            Notes = entity.Notes,
            Paths = paths,
        };
    }
}
