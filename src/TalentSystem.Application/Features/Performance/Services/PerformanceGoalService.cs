using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Application.Features.Performance.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Performance.Services;

public sealed class PerformanceGoalService : IPerformanceGoalService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreatePerformanceGoalRequest> _createValidator;
    private readonly IValidator<UpdatePerformanceGoalRequest> _updateValidator;
    private readonly IValidator<PerformanceGoalFilterRequest> _filterValidator;

    public PerformanceGoalService(
        TalentDbContext db,
        IValidator<CreatePerformanceGoalRequest> createValidator,
        IValidator<UpdatePerformanceGoalRequest> updateValidator,
        IValidator<PerformanceGoalFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PerformanceGoalDto>> CreateAsync(
        CreatePerformanceGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceGoalDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var employeeExists = await _db.Employees.AsNoTracking()
            .AnyAsync(x => x.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
        {
            return Result<PerformanceGoalDto>.Fail(
                "The employee was not found.",
                PerformanceErrors.EmployeeNotFound);
        }

        var cycle = await _db.PerformanceCycles.FirstOrDefaultAsync(
            x => x.Id == request.PerformanceCycleId,
            cancellationToken);
        if (cycle is null)
        {
            return Result<PerformanceGoalDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsGoalMutations(cycle.Status))
        {
            return Result<PerformanceGoalDto>.Fail(
                "Goals cannot be created for a closed or archived performance cycle.",
                PerformanceErrors.GoalCycleNotOpen);
        }

        if (await WouldExceedWeightCapAsync(
                request.EmployeeId,
                request.PerformanceCycleId,
                excludeGoalId: null,
                additionalWeight: request.Weight,
                additionalStatus: request.Status,
                cancellationToken))
        {
            return Result<PerformanceGoalDto>.Fail(
                "The total weight of draft and active goals for this employee in this cycle cannot exceed 100.",
                PerformanceErrors.GoalWeightExceeded);
        }

        var entity = new PerformanceGoal
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            TitleAr = request.TitleAr.Trim(),
            TitleEn = request.TitleEn.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Weight = request.Weight,
            TargetValue = string.IsNullOrWhiteSpace(request.TargetValue) ? null : request.TargetValue.Trim(),
            Status = request.Status
        };

        _db.PerformanceGoals.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceGoalDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceGoalDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceGoalDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.PerformanceGoals
            .Include(x => x.PerformanceCycle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PerformanceGoalDto>.Fail(
                "The performance goal was not found.",
                PerformanceErrors.GoalNotFound);
        }

        if (entity.Status is PerformanceGoalStatus.Completed or PerformanceGoalStatus.Cancelled)
        {
            return Result<PerformanceGoalDto>.Fail(
                "Completed or cancelled goals are read-only.",
                PerformanceErrors.GoalReadOnly);
        }

        if (!CycleAllowsGoalMutations(entity.PerformanceCycle.Status))
        {
            return Result<PerformanceGoalDto>.Fail(
                "Goals cannot be modified for a closed or archived performance cycle.",
                PerformanceErrors.GoalCycleNotOpen);
        }

        if (await WouldExceedWeightCapAsync(
                entity.EmployeeId,
                entity.PerformanceCycleId,
                excludeGoalId: entity.Id,
                additionalWeight: request.Weight,
                additionalStatus: request.Status,
                cancellationToken))
        {
            return Result<PerformanceGoalDto>.Fail(
                "The total weight of draft and active goals for this employee in this cycle cannot exceed 100.",
                PerformanceErrors.GoalWeightExceeded);
        }

        entity.TitleAr = request.TitleAr.Trim();
        entity.TitleEn = request.TitleEn.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.Weight = request.Weight;
        entity.TargetValue = string.IsNullOrWhiteSpace(request.TargetValue) ? null : request.TargetValue.Trim();
        entity.Status = request.Status;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceGoalDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceGoalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PerformanceGoals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PerformanceGoalDto>.Fail(
                "The performance goal was not found.",
                PerformanceErrors.GoalNotFound);
        }

        return Result<PerformanceGoalDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<PerformanceGoalDto>>> GetPagedAsync(
        PerformanceGoalFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<PerformanceGoalDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<PerformanceGoal> query = _db.PerformanceGoals.AsNoTracking();

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        if (request.PerformanceCycleId is { } cycleId && cycleId != Guid.Empty)
        {
            query = query.Where(x => x.PerformanceCycleId == cycleId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.TitleEn.Contains(term) ||
                x.TitleAr.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.TitleEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PerformanceGoalDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                PerformanceCycleId = x.PerformanceCycleId,
                TitleAr = x.TitleAr,
                TitleEn = x.TitleEn,
                Description = x.Description,
                Weight = x.Weight,
                TargetValue = x.TargetValue,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<PerformanceGoalDto>>.Ok(new PagedResult<PerformanceGoalDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static bool CycleAllowsGoalMutations(PerformanceCycleStatus status) =>
        status is PerformanceCycleStatus.Draft or PerformanceCycleStatus.Active;

    private static bool StatusCountsTowardWeightCap(PerformanceGoalStatus status) =>
        status is PerformanceGoalStatus.Draft or PerformanceGoalStatus.Active;

    private async Task<bool> WouldExceedWeightCapAsync(
        Guid employeeId,
        Guid performanceCycleId,
        Guid? excludeGoalId,
        decimal additionalWeight,
        PerformanceGoalStatus additionalStatus,
        CancellationToken cancellationToken)
    {
        var query = _db.PerformanceGoals.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && x.PerformanceCycleId == performanceCycleId);

        if (excludeGoalId.HasValue)
        {
            query = query.Where(x => x.Id != excludeGoalId.Value);
        }

        var othersSum = await query
            .Where(x => StatusCountsTowardWeightCap(x.Status))
            .SumAsync(x => x.Weight, cancellationToken);

        var additionalContribution = StatusCountsTowardWeightCap(additionalStatus) ? additionalWeight : 0m;

        return othersSum + additionalContribution > 100m;
    }

    private static PerformanceGoalDto MapToDto(PerformanceGoal entity) =>
        new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            TitleAr = entity.TitleAr,
            TitleEn = entity.TitleEn,
            Description = entity.Description,
            Weight = entity.Weight,
            TargetValue = entity.TargetValue,
            Status = entity.Status
        };
}
