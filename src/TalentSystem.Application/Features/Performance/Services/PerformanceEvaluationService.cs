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

public sealed class PerformanceEvaluationService : IPerformanceEvaluationService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreatePerformanceEvaluationRequest> _createValidator;
    private readonly IValidator<UpdatePerformanceEvaluationRequest> _updateValidator;
    private readonly IValidator<PerformanceEvaluationFilterRequest> _filterValidator;

    public PerformanceEvaluationService(
        TalentDbContext db,
        IValidator<CreatePerformanceEvaluationRequest> createValidator,
        IValidator<UpdatePerformanceEvaluationRequest> updateValidator,
        IValidator<PerformanceEvaluationFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PerformanceEvaluationDto>> CreateAsync(
        CreatePerformanceEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceEvaluationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var employeeExists = await _db.Employees.AsNoTracking()
            .AnyAsync(x => x.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "The employee was not found.",
                PerformanceErrors.EmployeeNotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsEvaluationMutations(cycle.Status))
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "Evaluations cannot be created for a closed or archived performance cycle.",
                PerformanceErrors.EvaluationCycleClosed);
        }

        if (await _db.PerformanceEvaluations.AsNoTracking().AnyAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken))
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "An evaluation already exists for this employee in the selected cycle.",
                PerformanceErrors.EvaluationDuplicate);
        }

        var entity = new PerformanceEvaluation
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            OverallScore = request.OverallScore,
            ManagerComments = string.IsNullOrWhiteSpace(request.ManagerComments)
                ? null
                : request.ManagerComments.Trim(),
            EmployeeComments = string.IsNullOrWhiteSpace(request.EmployeeComments)
                ? null
                : request.EmployeeComments.Trim(),
            Status = request.Status,
            EvaluatedOnUtc = null
        };

        if (entity.Status == PerformanceEvaluationStatus.Finalized)
        {
            entity.EvaluatedOnUtc = DateTime.UtcNow;
        }

        _db.PerformanceEvaluations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceEvaluationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceEvaluationDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceEvaluationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.PerformanceEvaluations
            .Include(x => x.PerformanceCycle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "The performance evaluation was not found.",
                PerformanceErrors.EvaluationNotFound);
        }

        if (entity.Status == PerformanceEvaluationStatus.Finalized)
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "Finalized evaluations are read-only.",
                PerformanceErrors.EvaluationReadOnly);
        }

        if (!CycleAllowsEvaluationMutations(entity.PerformanceCycle.Status))
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "Evaluations cannot be modified for a closed or archived performance cycle.",
                PerformanceErrors.EvaluationCycleClosed);
        }

        var previousStatus = entity.Status;

        entity.OverallScore = request.OverallScore;
        entity.ManagerComments = string.IsNullOrWhiteSpace(request.ManagerComments)
            ? null
            : request.ManagerComments.Trim();
        entity.EmployeeComments = string.IsNullOrWhiteSpace(request.EmployeeComments)
            ? null
            : request.EmployeeComments.Trim();
        entity.Status = request.Status;

        if (previousStatus != PerformanceEvaluationStatus.Finalized &&
            entity.Status == PerformanceEvaluationStatus.Finalized)
        {
            entity.EvaluatedOnUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceEvaluationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceEvaluationDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.PerformanceEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PerformanceEvaluationDto>.Fail(
                "The performance evaluation was not found.",
                PerformanceErrors.EvaluationNotFound);
        }

        return Result<PerformanceEvaluationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<PerformanceEvaluationDto>>> GetPagedAsync(
        PerformanceEvaluationFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<PerformanceEvaluationDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<PerformanceEvaluation> query = _db.PerformanceEvaluations.AsNoTracking();

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
            .Select(x => new PerformanceEvaluationDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                PerformanceCycleId = x.PerformanceCycleId,
                OverallScore = x.OverallScore,
                ManagerComments = x.ManagerComments,
                EmployeeComments = x.EmployeeComments,
                Status = x.Status,
                EvaluatedOnUtc = x.EvaluatedOnUtc
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<PerformanceEvaluationDto>>.Ok(new PagedResult<PerformanceEvaluationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static bool CycleAllowsEvaluationMutations(PerformanceCycleStatus status) =>
        status is PerformanceCycleStatus.Draft or PerformanceCycleStatus.Active;

    private static PerformanceEvaluationDto MapToDto(PerformanceEvaluation entity) =>
        new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            OverallScore = entity.OverallScore,
            ManagerComments = entity.ManagerComments,
            EmployeeComments = entity.EmployeeComments,
            Status = entity.Status,
            EvaluatedOnUtc = entity.EvaluatedOnUtc
        };
}
