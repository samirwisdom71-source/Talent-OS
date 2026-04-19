using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Potential.DTOs;
using TalentSystem.Application.Features.Potential.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Domain.Potential;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Potential.Services;

public sealed class PotentialAssessmentService : IPotentialAssessmentService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreatePotentialAssessmentRequest> _createValidator;
    private readonly IValidator<UpdatePotentialAssessmentRequest> _updateValidator;
    private readonly IValidator<PotentialAssessmentFilterRequest> _filterValidator;

    public PotentialAssessmentService(
        TalentDbContext db,
        IValidator<CreatePotentialAssessmentRequest> createValidator,
        IValidator<UpdatePotentialAssessmentRequest> updateValidator,
        IValidator<PotentialAssessmentFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PotentialAssessmentDto>> CreateAsync(
        CreatePotentialAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PotentialAssessmentDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The employee was not found.",
                PerformanceErrors.EmployeeNotFound);
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.AssessedByEmployeeId, cancellationToken))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The assessing employee was not found.",
                PotentialErrors.AssessorNotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsPotentialWork(cycle.Status))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "Potential assessments cannot be created for a closed or archived performance cycle.",
                PotentialErrors.AssessmentCycleClosed);
        }

        if (await _db.PotentialAssessments.AsNoTracking().AnyAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "A potential assessment already exists for this employee in the selected cycle.",
                PotentialErrors.AssessmentDuplicate);
        }

        var (overall, level) = ComputeOverallAndLevel(
            request.AgilityScore,
            request.LeadershipScore,
            request.GrowthScore,
            request.MobilityScore);

        var entity = new PotentialAssessment
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            AssessedByEmployeeId = request.AssessedByEmployeeId,
            AgilityScore = request.AgilityScore,
            LeadershipScore = request.LeadershipScore,
            GrowthScore = request.GrowthScore,
            MobilityScore = request.MobilityScore,
            OverallPotentialScore = overall,
            PotentialLevel = level,
            Comments = string.IsNullOrWhiteSpace(request.Comments) ? null : request.Comments.Trim(),
            Status = request.Status,
            AssessedOnUtc = request.Status == PotentialAssessmentStatus.Finalized
                ? DateTime.UtcNow
                : null
        };

        _db.PotentialAssessments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        AddFactorEntities(entity.Id, request.Factors);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await MapToDtoAsync(entity.Id, cancellationToken);
        if (created is null)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The potential assessment could not be loaded after creation.",
                PotentialErrors.AssessmentNotFound);
        }

        return Result<PotentialAssessmentDto>.Ok(created);
    }

    public async Task<Result<PotentialAssessmentDto>> UpdateAsync(
        Guid id,
        UpdatePotentialAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PotentialAssessmentDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.PotentialAssessments
            .Include(x => x.PerformanceCycle)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The potential assessment was not found.",
                PotentialErrors.AssessmentNotFound);
        }

        if (entity.Status == PotentialAssessmentStatus.Finalized)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "Finalized potential assessments are read-only.",
                PotentialErrors.AssessmentReadOnly);
        }

        if (!CycleAllowsPotentialWork(entity.PerformanceCycle.Status))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "Potential assessments cannot be modified for a closed or archived performance cycle.",
                PotentialErrors.AssessmentCycleClosed);
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.AssessedByEmployeeId, cancellationToken))
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The assessing employee was not found.",
                PotentialErrors.AssessorNotFound);
        }

        var previousStatus = entity.Status;

        entity.AssessedByEmployeeId = request.AssessedByEmployeeId;
        entity.AgilityScore = request.AgilityScore;
        entity.LeadershipScore = request.LeadershipScore;
        entity.GrowthScore = request.GrowthScore;
        entity.MobilityScore = request.MobilityScore;
        entity.Comments = string.IsNullOrWhiteSpace(request.Comments) ? null : request.Comments.Trim();
        entity.Status = request.Status;

        var (overall, level) = ComputeOverallAndLevel(
            entity.AgilityScore,
            entity.LeadershipScore,
            entity.GrowthScore,
            entity.MobilityScore);

        entity.OverallPotentialScore = overall;
        entity.PotentialLevel = level;

        if (previousStatus != PotentialAssessmentStatus.Finalized &&
            entity.Status == PotentialAssessmentStatus.Finalized)
        {
            entity.AssessedOnUtc = DateTime.UtcNow;
        }

        await SoftDeleteExistingFactorsAsync(entity.Id, cancellationToken);
        AddFactorEntities(entity.Id, request.Factors);

        await _db.SaveChangesAsync(cancellationToken);

        var updated = await MapToDtoAsync(entity.Id, cancellationToken);
        if (updated is null)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The potential assessment could not be loaded after update.",
                PotentialErrors.AssessmentNotFound);
        }

        return Result<PotentialAssessmentDto>.Ok(updated);
    }

    public async Task<Result<PotentialAssessmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await MapToDtoAsync(id, cancellationToken);
        if (dto is null)
        {
            return Result<PotentialAssessmentDto>.Fail(
                "The potential assessment was not found.",
                PotentialErrors.AssessmentNotFound);
        }

        return Result<PotentialAssessmentDto>.Ok(dto);
    }

    public async Task<Result<PagedResult<PotentialAssessmentDto>>> GetPagedAsync(
        PotentialAssessmentFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<PotentialAssessmentDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<PotentialAssessment> query = _db.PotentialAssessments.AsNoTracking();

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        if (request.PerformanceCycleId is { } cycleId && cycleId != Guid.Empty)
        {
            query = query.Where(x => x.PerformanceCycleId == cycleId);
        }

        if (request.PotentialLevel.HasValue)
        {
            query = query.Where(x => x.PotentialLevel == request.PotentialLevel.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ids = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var items = new List<PotentialAssessmentDto>();
        foreach (var assessmentId in ids)
        {
            var dto = await MapToDtoAsync(assessmentId, cancellationToken);
            if (dto is not null)
            {
                items.Add(dto);
            }
        }

        return Result<PagedResult<PotentialAssessmentDto>>.Ok(new PagedResult<PotentialAssessmentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private async Task SoftDeleteExistingFactorsAsync(Guid assessmentId, CancellationToken cancellationToken)
    {
        var existing = await _db.PotentialAssessmentFactors
            .Where(x => x.PotentialAssessmentId == assessmentId)
            .ToListAsync(cancellationToken);

        foreach (var factor in existing)
        {
            factor.RecordStatus = RecordStatus.Deleted;
        }
    }

    private void AddFactorEntities(Guid assessmentId, IReadOnlyList<PotentialAssessmentFactorItemDto> factors)
    {
        foreach (var item in factors)
        {
            _db.PotentialAssessmentFactors.Add(new PotentialAssessmentFactor
            {
                PotentialAssessmentId = assessmentId,
                FactorName = item.FactorName.Trim(),
                Score = item.Score,
                Weight = item.Weight,
                Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim()
            });
        }
    }

    private static bool CycleAllowsPotentialWork(PerformanceCycleStatus status) =>
        status is PerformanceCycleStatus.Draft or PerformanceCycleStatus.Active;

    private static (decimal Overall, PotentialLevel Level) ComputeOverallAndLevel(
        decimal agility,
        decimal leadership,
        decimal growth,
        decimal mobility)
    {
        var overall = Math.Round((agility + leadership + growth + mobility) / 4m, 2, MidpointRounding.AwayFromZero);
        var level = ResolvePotentialLevel(overall);
        return (overall, level);
    }

    private static PotentialLevel ResolvePotentialLevel(decimal overall)
    {
        if (overall < 50m)
        {
            return PotentialLevel.Low;
        }

        if (overall < 75m)
        {
            return PotentialLevel.Medium;
        }

        return PotentialLevel.High;
    }

    private async Task<PotentialAssessmentDto?> MapToDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.PotentialAssessments.AsNoTracking()
            .Include(x => x.Factors)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new PotentialAssessmentDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            AssessedByEmployeeId = entity.AssessedByEmployeeId,
            AgilityScore = entity.AgilityScore,
            LeadershipScore = entity.LeadershipScore,
            GrowthScore = entity.GrowthScore,
            MobilityScore = entity.MobilityScore,
            OverallPotentialScore = entity.OverallPotentialScore,
            PotentialLevel = entity.PotentialLevel,
            Comments = entity.Comments,
            Status = entity.Status,
            AssessedOnUtc = entity.AssessedOnUtc,
            Factors = entity.Factors
                .OrderBy(x => x.FactorName)
                .Select(x => new PotentialAssessmentFactorDto
                {
                    Id = x.Id,
                    FactorName = x.FactorName,
                    Score = x.Score,
                    Weight = x.Weight,
                    Notes = x.Notes
                })
                .ToList()
        };
    }
}
