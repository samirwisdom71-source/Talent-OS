using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Application.Features.Scoring.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Scoring;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Scoring.Services;

public sealed class TalentScoreService : ITalentScoreService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CalculateTalentScoreRequest> _calculateValidator;
    private readonly IValidator<RecalculateTalentScoreRequest> _recalculateValidator;
    private readonly IValidator<TalentScoreFilterRequest> _filterValidator;

    public TalentScoreService(
        TalentDbContext db,
        IValidator<CalculateTalentScoreRequest> calculateValidator,
        IValidator<RecalculateTalentScoreRequest> recalculateValidator,
        IValidator<TalentScoreFilterRequest> filterValidator)
    {
        _db = db;
        _calculateValidator = calculateValidator;
        _recalculateValidator = recalculateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<TalentScoreDto>> CalculateAsync(
        CalculateTalentScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _calculateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<TalentScoreDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<TalentScoreDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<TalentScoreDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsScoring(cycle.Status))
        {
            return Result<TalentScoreDto>.Fail(
                "Talent scores cannot be calculated for an archived performance cycle.",
                ScoringErrors.CycleArchivedCannotScore);
        }

        if (await _db.TalentScores.AsNoTracking().AnyAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken))
        {
            return Result<TalentScoreDto>.Fail(
                "A talent score already exists for this employee in the selected cycle.",
                ScoringErrors.TalentScoreAlreadyExists);
        }

        var resolved = await ResolveScoringInputsAsync(
            request.EmployeeId,
            request.PerformanceCycleId,
            cancellationToken);
        if (resolved.IsFailure)
        {
            return Result<TalentScoreDto>.Fail(resolved.Errors.ToList(), resolved.FailureCode);
        }

        var (performanceScore, potentialScore, performanceWeight, potentialWeight, calculationVersion) =
            resolved.Value!;

        var finalScore = ComputeFinalScore(performanceScore, potentialScore, performanceWeight, potentialWeight);

        var entity = new TalentScore
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            PerformanceScore = performanceScore,
            PotentialScore = potentialScore,
            FinalScore = finalScore,
            PerformanceWeight = performanceWeight,
            PotentialWeight = potentialWeight,
            CalculationVersion = calculationVersion,
            CalculatedOnUtc = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.TalentScores.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<TalentScoreDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentScoreDto>> RecalculateAsync(
        RecalculateTalentScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _recalculateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<TalentScoreDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<TalentScoreDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<TalentScoreDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsScoring(cycle.Status))
        {
            return Result<TalentScoreDto>.Fail(
                "Talent scores cannot be recalculated for an archived performance cycle.",
                ScoringErrors.CycleArchivedCannotScore);
        }

        var entity = await _db.TalentScores
            .FirstOrDefaultAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken);

        if (entity is null)
        {
            return Result<TalentScoreDto>.Fail(
                "The talent score was not found.",
                ScoringErrors.TalentScoreNotFound);
        }

        var resolved = await ResolveScoringInputsAsync(
            request.EmployeeId,
            request.PerformanceCycleId,
            cancellationToken);
        if (resolved.IsFailure)
        {
            return Result<TalentScoreDto>.Fail(resolved.Errors.ToList(), resolved.FailureCode);
        }

        var (performanceScore, potentialScore, performanceWeight, potentialWeight, calculationVersion) =
            resolved.Value!;

        entity.PerformanceScore = performanceScore;
        entity.PotentialScore = potentialScore;
        entity.PerformanceWeight = performanceWeight;
        entity.PotentialWeight = potentialWeight;
        entity.CalculationVersion = calculationVersion;
        entity.FinalScore = ComputeFinalScore(performanceScore, potentialScore, performanceWeight, potentialWeight);
        entity.CalculatedOnUtc = DateTime.UtcNow;
        if (request.Notes is not null)
        {
            entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TalentScoreDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentScoreDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TalentScores.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<TalentScoreDto>.Fail(
                "The talent score was not found.",
                ScoringErrors.TalentScoreNotFound);
        }

        return Result<TalentScoreDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentScoreDto>> GetByEmployeeAndCycleAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty || performanceCycleId == Guid.Empty)
        {
            return Result<TalentScoreDto>.Fail(
                "employeeId and performanceCycleId are required and cannot be empty.");
        }

        var entity = await _db.TalentScores.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId && x.PerformanceCycleId == performanceCycleId,
                cancellationToken);

        if (entity is null)
        {
            return Result<TalentScoreDto>.Fail(
                "The talent score was not found.",
                ScoringErrors.TalentScoreNotFound);
        }

        return Result<TalentScoreDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<TalentScoreDto>>> GetPagedAsync(
        TalentScoreFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<TalentScoreDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<TalentScore> query = _db.TalentScores.AsNoTracking();

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        if (request.PerformanceCycleId is { } cycleId && cycleId != Guid.Empty)
        {
            query = query.Where(x => x.PerformanceCycleId == cycleId);
        }

        if (request.MinFinalScore is { } minScore)
        {
            query = query.Where(x => x.FinalScore >= minScore);
        }

        if (request.MaxFinalScore is { } maxScore)
        {
            query = query.Where(x => x.FinalScore <= maxScore);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CalculatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TalentScoreDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                PerformanceCycleId = x.PerformanceCycleId,
                PerformanceScore = x.PerformanceScore,
                PotentialScore = x.PotentialScore,
                FinalScore = x.FinalScore,
                PerformanceWeight = x.PerformanceWeight,
                PotentialWeight = x.PotentialWeight,
                CalculationVersion = x.CalculationVersion,
                CalculatedOnUtc = x.CalculatedOnUtc,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<TalentScoreDto>>.Ok(new PagedResult<TalentScoreDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private async Task<Result<(decimal PerformanceScore, decimal PotentialScore, decimal PerformanceWeight, decimal PotentialWeight, string CalculationVersion)>> ResolveScoringInputsAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken)
    {
        var evaluation = await _db.PerformanceEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId && x.PerformanceCycleId == performanceCycleId,
                cancellationToken);

        if (evaluation is null)
        {
            return Result<(decimal, decimal, decimal, decimal, string)>.Fail(
                "No performance evaluation exists for this employee in the selected cycle.",
                ScoringErrors.MissingPerformanceEvaluation);
        }

        if (evaluation.Status != PerformanceEvaluationStatus.Finalized)
        {
            return Result<(decimal, decimal, decimal, decimal, string)>.Fail(
                "The performance evaluation must be finalized before calculating a talent score.",
                ScoringErrors.SourceNotFinalized);
        }

        var assessment = await _db.PotentialAssessments.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId && x.PerformanceCycleId == performanceCycleId,
                cancellationToken);

        if (assessment is null)
        {
            return Result<(decimal, decimal, decimal, decimal, string)>.Fail(
                "No potential assessment exists for this employee in the selected cycle.",
                ScoringErrors.MissingPotentialAssessment);
        }

        if (assessment.Status != PotentialAssessmentStatus.Finalized)
        {
            return Result<(decimal, decimal, decimal, decimal, string)>.Fail(
                "The potential assessment must be finalized before calculating a talent score.",
                ScoringErrors.SourceNotFinalized);
        }

        var policy = await _db.ScoringPolicies.AsNoTracking()
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .OrderByDescending(x => x.EffectiveFromUtc)
            .FirstOrDefaultAsync(cancellationToken);

        decimal performanceWeight;
        decimal potentialWeight;
        string calculationVersion;

        if (policy is not null)
        {
            performanceWeight = policy.PerformanceWeight;
            potentialWeight = policy.PotentialWeight;
            calculationVersion = policy.Version;
        }
        else
        {
            performanceWeight = ScoringConstants.DefaultPerformanceWeight;
            potentialWeight = ScoringConstants.DefaultPotentialWeight;
            calculationVersion = ScoringConstants.DefaultCalculationVersion;
        }

        return Result<(decimal, decimal, decimal, decimal, string)>.Ok((
            evaluation.OverallScore,
            assessment.OverallPotentialScore,
            performanceWeight,
            potentialWeight,
            calculationVersion));
    }

    private static decimal ComputeFinalScore(
        decimal performanceScore,
        decimal potentialScore,
        decimal performanceWeight,
        decimal potentialWeight) =>
        Math.Round(
            (performanceScore * performanceWeight + potentialScore * potentialWeight) / 100m,
            2,
            MidpointRounding.AwayFromZero);

    private static bool CycleAllowsScoring(PerformanceCycleStatus status) =>
        status is PerformanceCycleStatus.Draft
            or PerformanceCycleStatus.Active
            or PerformanceCycleStatus.Closed;

    private static TalentScoreDto MapToDto(TalentScore entity) =>
        new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            PerformanceScore = entity.PerformanceScore,
            PotentialScore = entity.PotentialScore,
            FinalScore = entity.FinalScore,
            PerformanceWeight = entity.PerformanceWeight,
            PotentialWeight = entity.PotentialWeight,
            CalculationVersion = entity.CalculationVersion,
            CalculatedOnUtc = entity.CalculatedOnUtc,
            Notes = entity.Notes
        };
}
