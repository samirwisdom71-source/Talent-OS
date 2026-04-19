using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Application.Features.Succession.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Succession.Services;

public sealed class SuccessionPlanService : ISuccessionPlanService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateSuccessionPlanRequest> _createValidator;
    private readonly IValidator<UpdateSuccessionPlanRequest> _updateValidator;
    private readonly IValidator<SuccessionPlanFilterRequest> _filterValidator;

    public SuccessionPlanService(
        TalentDbContext db,
        IValidator<CreateSuccessionPlanRequest> createValidator,
        IValidator<UpdateSuccessionPlanRequest> updateValidator,
        IValidator<SuccessionPlanFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<SuccessionPlanDto>> CreateAsync(
        CreateSuccessionPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SuccessionPlanDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var critical = await _db.CriticalPositions
            .FirstOrDefaultAsync(x => x.Id == request.CriticalPositionId, cancellationToken);
        if (critical is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The critical position was not found.",
                SuccessionErrors.CriticalPositionNotFound);
        }

        if (critical.RecordStatus != RecordStatus.Active)
        {
            return Result<SuccessionPlanDto>.Fail(
                "Succession plans can only be created for an active critical position.",
                SuccessionErrors.CriticalPositionInactive);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (cycle.Status == PerformanceCycleStatus.Archived)
        {
            return Result<SuccessionPlanDto>.Fail(
                "Succession plans cannot be created for an archived performance cycle.",
                SuccessionErrors.CycleArchivedCannotCreatePlan);
        }

        if (await _db.SuccessionPlans.AsNoTracking().AnyAsync(
                x => x.CriticalPositionId == request.CriticalPositionId &&
                     x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken))
        {
            return Result<SuccessionPlanDto>.Fail(
                "A succession plan already exists for this critical position in the selected cycle.",
                SuccessionErrors.SuccessionPlanDuplicate);
        }

        var entity = new SuccessionPlan
        {
            CriticalPositionId = request.CriticalPositionId,
            PerformanceCycleId = request.PerformanceCycleId,
            PlanName = request.PlanName.Trim(),
            Status = SuccessionPlanStatus.Draft,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.SuccessionPlans.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await SuccessionCoverageSnapshotRefresher.RefreshAsync(_db, entity.Id, cancellationToken);

        return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<SuccessionPlanDto>> UpdateAsync(
        Guid id,
        UpdateSuccessionPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SuccessionPlanDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.SuccessionPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan was not found.",
                SuccessionErrors.SuccessionPlanNotFound);
        }

        if (!PlanAllowsMetadataUpdates(entity.Status))
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan is closed or archived and cannot be updated.",
                SuccessionErrors.SuccessionPlanReadOnly);
        }

        entity.PlanName = request.PlanName.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<SuccessionPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessionPlans.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan was not found.",
                SuccessionErrors.SuccessionPlanNotFound);
        }

        return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<PagedResult<SuccessionPlanDto>>> GetPagedAsync(
        SuccessionPlanFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<SuccessionPlanDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<SuccessionPlan> query = _db.SuccessionPlans.AsNoTracking();

        if (request.CriticalPositionId is { } cpId && cpId != Guid.Empty)
        {
            query = query.Where(x => x.CriticalPositionId == cpId);
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
            .Select(x => new SuccessionPlanDto
            {
                Id = x.Id,
                CriticalPositionId = x.CriticalPositionId,
                PerformanceCycleId = x.PerformanceCycleId,
                PlanName = x.PlanName,
                Status = x.Status,
                Notes = x.Notes,
                CoverageSnapshot = null
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<SuccessionPlanDto>>.Ok(new PagedResult<SuccessionPlanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<SuccessionPlanDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessionPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan was not found.",
                SuccessionErrors.SuccessionPlanNotFound);
        }

        if (entity.Status == SuccessionPlanStatus.Active)
        {
            return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
        }

        if (entity.Status != SuccessionPlanStatus.Draft)
        {
            return Result<SuccessionPlanDto>.Fail(
                "Only a draft succession plan can be activated.",
                SuccessionErrors.InvalidPlanStatusTransition);
        }

        entity.Status = SuccessionPlanStatus.Active;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<SuccessionPlanDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessionPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan was not found.",
                SuccessionErrors.SuccessionPlanNotFound);
        }

        if (entity.Status == SuccessionPlanStatus.Closed || entity.Status == SuccessionPlanStatus.Archived)
        {
            return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
        }

        if (entity.Status is not (SuccessionPlanStatus.Draft or SuccessionPlanStatus.Active))
        {
            return Result<SuccessionPlanDto>.Fail(
                "The succession plan cannot be closed from its current status.",
                SuccessionErrors.InvalidPlanStatusTransition);
        }

        entity.Status = SuccessionPlanStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<SuccessionPlanDto>.Ok(await MapToDtoAsync(entity, cancellationToken));
    }

    private static bool PlanAllowsMetadataUpdates(SuccessionPlanStatus status) =>
        status is SuccessionPlanStatus.Draft or SuccessionPlanStatus.Active;

    private async Task<SuccessionPlanDto> MapToDtoAsync(SuccessionPlan entity, CancellationToken cancellationToken)
    {
        var dto = new SuccessionPlanDto
        {
            Id = entity.Id,
            CriticalPositionId = entity.CriticalPositionId,
            PerformanceCycleId = entity.PerformanceCycleId,
            PlanName = entity.PlanName,
            Status = entity.Status,
            Notes = entity.Notes
        };

        var snap = await _db.SuccessionCoverageSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SuccessionPlanId == entity.Id, cancellationToken);

        if (snap is not null)
        {
            dto.CoverageSnapshot = new SuccessionCoverageSnapshotDto
            {
                Id = snap.Id,
                SuccessionPlanId = snap.SuccessionPlanId,
                TotalCandidates = snap.TotalCandidates,
                HasReadyNow = snap.HasReadyNow,
                HasPrimarySuccessor = snap.HasPrimarySuccessor,
                CoverageScore = snap.CoverageScore,
                CalculatedOnUtc = snap.CalculatedOnUtc
            };
        }

        return dto;
    }
}
