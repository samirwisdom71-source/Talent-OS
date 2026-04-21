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

public sealed class SuccessorCandidateService : ISuccessorCandidateService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateSuccessorCandidateRequest> _createValidator;
    private readonly IValidator<UpdateSuccessorCandidateRequest> _updateValidator;
    private readonly IValidator<SuccessorCandidateFilterRequest> _filterValidator;

    public SuccessorCandidateService(
        TalentDbContext db,
        IValidator<CreateSuccessorCandidateRequest> createValidator,
        IValidator<UpdateSuccessorCandidateRequest> updateValidator,
        IValidator<SuccessorCandidateFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<SuccessorCandidateDto>> AddAsync(
        CreateSuccessorCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SuccessorCandidateDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var plan = await _db.SuccessionPlans
            .Include(x => x.CriticalPosition)
            .FirstOrDefaultAsync(x => x.Id == request.SuccessionPlanId, cancellationToken);

        if (plan is null)
        {
            return Result<SuccessorCandidateDto>.Fail(
                "The succession plan was not found.",
                SuccessionErrors.SuccessionPlanNotFound);
        }

        if (!PlanAllowsCandidateMutations(plan.Status))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "Successor candidates cannot be modified when the succession plan is closed or archived.",
                SuccessionErrors.SuccessionPlanReadOnly);
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var employee = await _db.Employees.AsNoTracking()
            .FirstAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee.PositionId == plan.CriticalPosition.PositionId)
        {
            return Result<SuccessorCandidateDto>.Fail(
                "A successor cannot currently hold the same target position as the critical role.",
                SuccessionErrors.CandidateOccupiesTargetPosition);
        }

        if (await _db.SuccessorCandidates.AsNoTracking().AnyAsync(
                x => x.SuccessionPlanId == request.SuccessionPlanId && x.EmployeeId == request.EmployeeId,
                cancellationToken))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "This employee is already a successor candidate on this plan.",
                SuccessionErrors.SuccessorCandidateDuplicate);
        }

        if (await _db.SuccessorCandidates.AsNoTracking().AnyAsync(
                x => x.SuccessionPlanId == request.SuccessionPlanId &&
                     x.RankOrder == request.RankOrder,
                cancellationToken))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "This rank order is already used by another candidate on this plan.",
                SuccessionErrors.SuccessorCandidateRankDuplicate);
        }

        if (request.IsPrimarySuccessor)
        {
            // Two-phase: unique index on SuccessionPlanId (where IsPrimarySuccessor) — one save per step
            // so SQL never has two "primary" rows for the same plan in a single command batch.
            await ClearPrimaryFlagsForPlanAsync(request.SuccessionPlanId, null, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var entity = new SuccessorCandidate
        {
            SuccessionPlanId = request.SuccessionPlanId,
            EmployeeId = request.EmployeeId,
            ReadinessLevel = request.ReadinessLevel,
            RankOrder = request.RankOrder,
            IsPrimarySuccessor = request.IsPrimarySuccessor,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.SuccessorCandidates.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await SuccessionCoverageSnapshotRefresher.RefreshAsync(_db, request.SuccessionPlanId, cancellationToken);

        return Result<SuccessorCandidateDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<SuccessorCandidateDto>> UpdateAsync(
        Guid id,
        UpdateSuccessorCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SuccessorCandidateDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.SuccessorCandidates
            .Include(x => x.SuccessionPlan)
            .ThenInclude(x => x.CriticalPosition)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<SuccessorCandidateDto>.Fail(
                "The successor candidate was not found.",
                SuccessionErrors.SuccessorCandidateNotFound);
        }

        if (!PlanAllowsCandidateMutations(entity.SuccessionPlan.Status))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "Successor candidates cannot be modified when the succession plan is closed or archived.",
                SuccessionErrors.SuccessionPlanReadOnly);
        }

        if (await _db.SuccessorCandidates.AsNoTracking().AnyAsync(
                x => x.SuccessionPlanId == entity.SuccessionPlanId &&
                     x.RankOrder == request.RankOrder &&
                     x.Id != id,
                cancellationToken))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "This rank order is already used by another candidate on this plan.",
                SuccessionErrors.SuccessorCandidateRankDuplicate);
        }

        if (request.IsPrimarySuccessor)
        {
            await ClearPrimaryFlagsForPlanAsync(entity.SuccessionPlanId, null, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        entity.ReadinessLevel = request.ReadinessLevel;
        entity.RankOrder = request.RankOrder;
        entity.IsPrimarySuccessor = request.IsPrimarySuccessor;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        await SuccessionCoverageSnapshotRefresher.RefreshAsync(_db, entity.SuccessionPlanId, cancellationToken);

        return Result<SuccessorCandidateDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<SuccessorCandidateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessorCandidates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<SuccessorCandidateDto>.Fail(
                "The successor candidate was not found.",
                SuccessionErrors.SuccessorCandidateNotFound);
        }

        return Result<SuccessorCandidateDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<SuccessorCandidateDto>>> GetPagedAsync(
        SuccessorCandidateFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<SuccessorCandidateDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<SuccessorCandidate> query = _db.SuccessorCandidates.AsNoTracking()
            .Where(x => x.SuccessionPlanId == request.SuccessionPlanId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.RankOrder)
            .ThenBy(x => x.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SuccessorCandidateDto
            {
                Id = x.Id,
                SuccessionPlanId = x.SuccessionPlanId,
                EmployeeId = x.EmployeeId,
                ReadinessLevel = x.ReadinessLevel,
                RankOrder = x.RankOrder,
                IsPrimarySuccessor = x.IsPrimarySuccessor,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<SuccessorCandidateDto>>.Ok(new PagedResult<SuccessorCandidateDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessorCandidates
            .Include(x => x.SuccessionPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.Fail(
                "The successor candidate was not found.",
                SuccessionErrors.SuccessorCandidateNotFound);
        }

        if (!PlanAllowsCandidateMutations(entity.SuccessionPlan.Status))
        {
            return Result.Fail(
                "Successor candidates cannot be modified when the succession plan is closed or archived.",
                SuccessionErrors.SuccessionPlanReadOnly);
        }

        var planId = entity.SuccessionPlanId;
        entity.RecordStatus = RecordStatus.Deleted;
        await _db.SaveChangesAsync(cancellationToken);

        await SuccessionCoverageSnapshotRefresher.RefreshAsync(_db, planId, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<SuccessorCandidateDto>> MarkPrimaryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.SuccessorCandidates
            .Include(x => x.SuccessionPlan)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<SuccessorCandidateDto>.Fail(
                "The successor candidate was not found.",
                SuccessionErrors.SuccessorCandidateNotFound);
        }

        if (!PlanAllowsCandidateMutations(entity.SuccessionPlan.Status))
        {
            return Result<SuccessorCandidateDto>.Fail(
                "Successor candidates cannot be modified when the succession plan is closed or archived.",
                SuccessionErrors.SuccessionPlanReadOnly);
        }

        if (entity.IsPrimarySuccessor)
        {
            return Result<SuccessorCandidateDto>.Ok(MapToDto(entity));
        }

        await ClearPrimaryFlagsForPlanAsync(entity.SuccessionPlanId, null, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        entity.IsPrimarySuccessor = true;
        await _db.SaveChangesAsync(cancellationToken);

        await SuccessionCoverageSnapshotRefresher.RefreshAsync(_db, entity.SuccessionPlanId, cancellationToken);

        return Result<SuccessorCandidateDto>.Ok(MapToDto(entity));
    }

    private static bool PlanAllowsCandidateMutations(SuccessionPlanStatus status) =>
        status is SuccessionPlanStatus.Draft or SuccessionPlanStatus.Active;

    private async Task ClearPrimaryFlagsForPlanAsync(
        Guid successionPlanId,
        Guid? exceptCandidateId,
        CancellationToken cancellationToken)
    {
        var primaries = await _db.SuccessorCandidates
            .Where(x => x.SuccessionPlanId == successionPlanId && x.IsPrimarySuccessor)
            .Where(x => exceptCandidateId == null || x.Id != exceptCandidateId.Value)
            .ToListAsync(cancellationToken);

        foreach (var p in primaries)
        {
            p.IsPrimarySuccessor = false;
        }
    }

    private static SuccessorCandidateDto MapToDto(SuccessorCandidate entity) =>
        new()
        {
            Id = entity.Id,
            SuccessionPlanId = entity.SuccessionPlanId,
            EmployeeId = entity.EmployeeId,
            ReadinessLevel = entity.ReadinessLevel,
            RankOrder = entity.RankOrder,
            IsPrimarySuccessor = entity.IsPrimarySuccessor,
            Notes = entity.Notes
        };
}
