using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Application.Features.Notifications.DTOs;
using TalentSystem.Application.Features.Notifications.Integration;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Services;

public sealed class DevelopmentPlanService : IDevelopmentPlanService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateDevelopmentPlanRequest> _createValidator;
    private readonly IValidator<UpdateDevelopmentPlanRequest> _updateValidator;
    private readonly IValidator<DevelopmentPlanFilterRequest> _filterValidator;
    private readonly IValidator<ActivateDevelopmentPlanRequest> _activateValidator;
    private readonly INotificationService _notifications;

    public DevelopmentPlanService(
        TalentDbContext db,
        IValidator<CreateDevelopmentPlanRequest> createValidator,
        IValidator<UpdateDevelopmentPlanRequest> updateValidator,
        IValidator<DevelopmentPlanFilterRequest> filterValidator,
        IValidator<ActivateDevelopmentPlanRequest> activateValidator,
        INotificationService notifications)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _activateValidator = activateValidator;
        _notifications = notifications;
    }

    public async Task<Result<DevelopmentPlanDto>> CreateAsync(
        CreateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (HasDuplicateLinks(request.Links))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "Duplicate link type and entity combinations are not allowed.",
                DevelopmentErrors.DevelopmentPlanLinkDuplicate);
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (cycle.Status == PerformanceCycleStatus.Archived)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "Development plans cannot be created for an archived performance cycle.",
                DevelopmentErrors.CycleArchivedCannotCreatePlan);
        }

        var entity = new DevelopmentPlan
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            PlanTitle = request.PlanTitle.Trim(),
            SourceType = request.SourceType,
            Status = DevelopmentPlanStatus.Draft,
            TargetCompletionDate = request.TargetCompletionDate,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        if (request.Links is { Count: > 0 })
        {
            foreach (var link in request.Links)
            {
                entity.Links.Add(new DevelopmentPlanLink
                {
                    LinkType = link.LinkType,
                    LinkedEntityId = link.LinkedEntityId,
                    Notes = string.IsNullOrWhiteSpace(link.Notes) ? null : link.Notes.Trim()
                });
            }
        }

        _db.DevelopmentPlans.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanDto>> UpdateAsync(
        Guid id,
        UpdateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (HasDuplicateLinks(request.Links))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "Duplicate link type and entity combinations are not allowed.",
                DevelopmentErrors.DevelopmentPlanLinkDuplicate);
        }

        var entity = await _db.DevelopmentPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        if (!PlanAllowsMetadataEdit(entity.Status))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan cannot be updated in its current status.",
                DevelopmentErrors.DevelopmentPlanReadOnly);
        }

        entity.PlanTitle = request.PlanTitle.Trim();
        entity.SourceType = request.SourceType;
        entity.TargetCompletionDate = request.TargetCompletionDate;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        if (request.Links is not null)
        {
            await ReplacePlanLinksAsync(entity.Id, request.Links, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _db.DevelopmentPlans.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(id, includeLinks: true, cancellationToken));
    }

    public async Task<Result<PagedResult<DevelopmentPlanDto>>> GetPagedAsync(
        DevelopmentPlanFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<DevelopmentPlanDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<DevelopmentPlan> query = _db.DevelopmentPlans.AsNoTracking();

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        if (request.PerformanceCycleId is { } cycleId && cycleId != Guid.Empty)
        {
            query = query.Where(x => x.PerformanceCycleId == cycleId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DevelopmentPlanDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                PerformanceCycleId = x.PerformanceCycleId,
                PlanTitle = x.PlanTitle,
                SourceType = x.SourceType,
                Status = x.Status,
                TargetCompletionDate = x.TargetCompletionDate,
                Notes = x.Notes,
                ApprovedByEmployeeId = x.ApprovedByEmployeeId,
                ApprovedOnUtc = x.ApprovedOnUtc,
                Links = null
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<DevelopmentPlanDto>>.Ok(new PagedResult<DevelopmentPlanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<DevelopmentPlanDto>> ActivateAsync(
        Guid id,
        ActivateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _activateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<DevelopmentPlanDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (request.ApprovedByEmployeeId is { } approverId &&
            !await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == approverId, cancellationToken))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The approving employee was not found.",
                EmployeeErrors.NotFound);
        }

        var entity = await _db.DevelopmentPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        if (entity.Status == DevelopmentPlanStatus.Active)
        {
            return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
        }

        if (entity.Status != DevelopmentPlanStatus.Draft)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "Only a draft development plan can be activated.",
                DevelopmentErrors.InvalidDevelopmentPlanStatusTransition);
        }

        entity.Status = DevelopmentPlanStatus.Active;
        entity.ApprovedByEmployeeId = request.ApprovedByEmployeeId;
        entity.ApprovedOnUtc = request.ApprovedByEmployeeId.HasValue ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(cancellationToken);

        await NotifyEmployeePlanAsync(
                entity.Id,
                entity.EmployeeId,
                entity.PlanTitle,
                DevelopmentNotificationRequests.ForPlanActivated,
                cancellationToken)
            .ConfigureAwait(false);

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanDto>> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        if (entity.Status == DevelopmentPlanStatus.Completed)
        {
            return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
        }

        if (entity.Status != DevelopmentPlanStatus.Active)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "Only an active development plan can be completed.",
                DevelopmentErrors.InvalidDevelopmentPlanStatusTransition);
        }

        entity.Status = DevelopmentPlanStatus.Completed;
        await _db.SaveChangesAsync(cancellationToken);

        await NotifyEmployeePlanAsync(
                entity.Id,
                entity.EmployeeId,
                entity.PlanTitle,
                DevelopmentNotificationRequests.ForPlanCompleted,
                cancellationToken)
            .ConfigureAwait(false);

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
    }

    public async Task<Result<DevelopmentPlanDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.DevelopmentPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan was not found.",
                DevelopmentErrors.DevelopmentPlanNotFound);
        }

        if (entity.Status == DevelopmentPlanStatus.Cancelled)
        {
            return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
        }

        if (entity.Status is not (DevelopmentPlanStatus.Draft or DevelopmentPlanStatus.Active))
        {
            return Result<DevelopmentPlanDto>.Fail(
                "The development plan cannot be cancelled from its current status.",
                DevelopmentErrors.InvalidDevelopmentPlanStatusTransition);
        }

        entity.Status = DevelopmentPlanStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DevelopmentPlanDto>.Ok(await MapToDtoAsync(entity.Id, includeLinks: true, cancellationToken));
    }

    private async Task NotifyEmployeePlanAsync(
        Guid planId,
        Guid employeeId,
        string planTitle,
        Func<Guid, Guid, string, CreateNotificationRequest> build,
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

        await _notifications.PublishIntegrationAsync(build(planId, userId.Value, planTitle), cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool HasDuplicateLinks(IReadOnlyList<DevelopmentPlanLinkInputDto>? links)
    {
        if (links is null || links.Count == 0)
        {
            return false;
        }

        var set = new HashSet<(DevelopmentPlanLinkType, Guid)>();
        foreach (var link in links)
        {
            if (!set.Add((link.LinkType, link.LinkedEntityId)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool PlanAllowsMetadataEdit(DevelopmentPlanStatus status) =>
        status is DevelopmentPlanStatus.Draft or DevelopmentPlanStatus.Active;

    private async Task ReplacePlanLinksAsync(
        Guid planId,
        IReadOnlyList<DevelopmentPlanLinkInputDto> links,
        CancellationToken cancellationToken)
    {
        var existing = await _db.DevelopmentPlanLinks
            .Where(x => x.DevelopmentPlanId == planId)
            .ToListAsync(cancellationToken);

        foreach (var link in existing)
        {
            link.RecordStatus = RecordStatus.Deleted;
        }

        foreach (var link in links)
        {
            _db.DevelopmentPlanLinks.Add(new DevelopmentPlanLink
            {
                DevelopmentPlanId = planId,
                LinkType = link.LinkType,
                LinkedEntityId = link.LinkedEntityId,
                Notes = string.IsNullOrWhiteSpace(link.Notes) ? null : link.Notes.Trim()
            });
        }
    }

    private async Task<DevelopmentPlanDto> MapToDtoAsync(
        Guid planId,
        bool includeLinks,
        CancellationToken cancellationToken)
    {
        var entity = await _db.DevelopmentPlans.AsNoTracking()
            .FirstAsync(x => x.Id == planId, cancellationToken);

        var dto = new DevelopmentPlanDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            PlanTitle = entity.PlanTitle,
            SourceType = entity.SourceType,
            Status = entity.Status,
            TargetCompletionDate = entity.TargetCompletionDate,
            Notes = entity.Notes,
            ApprovedByEmployeeId = entity.ApprovedByEmployeeId,
            ApprovedOnUtc = entity.ApprovedOnUtc
        };

        if (includeLinks)
        {
            dto.Links = await _db.DevelopmentPlanLinks.AsNoTracking()
                .Where(x => x.DevelopmentPlanId == planId)
                .OrderBy(x => x.LinkType)
                .ThenBy(x => x.LinkedEntityId)
                .Select(x => new DevelopmentPlanLinkDto
                {
                    Id = x.Id,
                    DevelopmentPlanId = x.DevelopmentPlanId,
                    LinkType = x.LinkType,
                    LinkedEntityId = x.LinkedEntityId,
                    Notes = x.Notes
                })
                .ToListAsync(cancellationToken);
        }

        return dto;
    }
}
