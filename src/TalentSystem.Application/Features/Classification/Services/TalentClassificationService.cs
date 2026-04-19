using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Application.Features.Classification.Interfaces;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Domain.Scoring;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Classification.Services;

public sealed class TalentClassificationService : ITalentClassificationService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<ClassifyTalentClassificationRequest> _classifyValidator;
    private readonly IValidator<ReclassifyTalentClassificationRequest> _reclassifyValidator;
    private readonly IValidator<TalentClassificationFilterRequest> _filterValidator;

    public TalentClassificationService(
        TalentDbContext db,
        IValidator<ClassifyTalentClassificationRequest> classifyValidator,
        IValidator<ReclassifyTalentClassificationRequest> reclassifyValidator,
        IValidator<TalentClassificationFilterRequest> filterValidator)
    {
        _db = db;
        _classifyValidator = classifyValidator;
        _reclassifyValidator = reclassifyValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<TalentClassificationDto>> ClassifyAsync(
        ClassifyTalentClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _classifyValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<TalentClassificationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<TalentClassificationDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsClassification(cycle.Status))
        {
            return Result<TalentClassificationDto>.Fail(
                "Talent classifications cannot be created for an archived performance cycle.",
                ClassificationErrors.CycleArchivedCannotClassify);
        }

        if (await _db.TalentClassifications.AsNoTracking().AnyAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken))
        {
            return Result<TalentClassificationDto>.Fail(
                "A talent classification already exists for this employee in the selected cycle.",
                ClassificationErrors.ClassificationAlreadyExists);
        }

        var talentScore = await _db.TalentScores.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken);

        if (talentScore is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "A talent score must exist for this employee and cycle before classification.",
                ClassificationErrors.TalentScoreRequired);
        }

        var (lowThreshold, highThreshold) = await GetActiveThresholdsAsync(cancellationToken);
        var computed = ComputeClassification(talentScore, lowThreshold, highThreshold);

        var entity = new TalentClassification
        {
            EmployeeId = request.EmployeeId,
            PerformanceCycleId = request.PerformanceCycleId,
            TalentScoreId = talentScore.Id,
            PerformanceBand = computed.PerformanceBand,
            PotentialBand = computed.PotentialBand,
            NineBoxCode = computed.NineBoxCode,
            CategoryName = computed.CategoryName,
            IsHighPotential = computed.IsHighPotential,
            IsHighPerformer = computed.IsHighPerformer,
            ClassifiedOnUtc = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.TalentClassifications.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<TalentClassificationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentClassificationDto>> ReclassifyAsync(
        ReclassifyTalentClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _reclassifyValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<TalentClassificationDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(x => x.Id == request.EmployeeId, cancellationToken))
        {
            return Result<TalentClassificationDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PerformanceCycleId, cancellationToken);
        if (cycle is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (!CycleAllowsClassification(cycle.Status))
        {
            return Result<TalentClassificationDto>.Fail(
                "Talent classifications cannot be recalculated for an archived performance cycle.",
                ClassificationErrors.CycleArchivedCannotClassify);
        }

        var entity = await _db.TalentClassifications
            .FirstOrDefaultAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken);

        if (entity is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "The talent classification was not found.",
                ClassificationErrors.ClassificationNotFound);
        }

        var talentScore = await _db.TalentScores.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == request.EmployeeId && x.PerformanceCycleId == request.PerformanceCycleId,
                cancellationToken);

        if (talentScore is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "A talent score must exist for this employee and cycle before reclassification.",
                ClassificationErrors.TalentScoreRequired);
        }

        var (lowThreshold, highThreshold) = await GetActiveThresholdsAsync(cancellationToken);
        var computed = ComputeClassification(talentScore, lowThreshold, highThreshold);

        entity.TalentScoreId = talentScore.Id;
        entity.PerformanceBand = computed.PerformanceBand;
        entity.PotentialBand = computed.PotentialBand;
        entity.NineBoxCode = computed.NineBoxCode;
        entity.CategoryName = computed.CategoryName;
        entity.IsHighPotential = computed.IsHighPotential;
        entity.IsHighPerformer = computed.IsHighPerformer;
        entity.ClassifiedOnUtc = DateTime.UtcNow;

        if (request.Notes is not null)
        {
            entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TalentClassificationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentClassificationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.TalentClassifications.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "The talent classification was not found.",
                ClassificationErrors.ClassificationNotFound);
        }

        return Result<TalentClassificationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<TalentClassificationDto>> GetByEmployeeAndCycleAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty || performanceCycleId == Guid.Empty)
        {
            return Result<TalentClassificationDto>.Fail(
                "employeeId and performanceCycleId are required and cannot be empty.");
        }

        var entity = await _db.TalentClassifications.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId && x.PerformanceCycleId == performanceCycleId,
                cancellationToken);

        if (entity is null)
        {
            return Result<TalentClassificationDto>.Fail(
                "The talent classification was not found.",
                ClassificationErrors.ClassificationNotFound);
        }

        return Result<TalentClassificationDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<TalentClassificationDto>>> GetPagedAsync(
        TalentClassificationFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<TalentClassificationDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<TalentClassification> query = _db.TalentClassifications.AsNoTracking();

        if (request.EmployeeId is { } employeeId && employeeId != Guid.Empty)
        {
            query = query.Where(x => x.EmployeeId == employeeId);
        }

        if (request.PerformanceCycleId is { } cycleId && cycleId != Guid.Empty)
        {
            query = query.Where(x => x.PerformanceCycleId == cycleId);
        }

        if (request.NineBoxCode is { } box)
        {
            query = query.Where(x => x.NineBoxCode == box);
        }

        if (request.IsHighPotential is { } hip)
        {
            query = query.Where(x => x.IsHighPotential == hip);
        }

        if (request.IsHighPerformer is { } hiperf)
        {
            query = query.Where(x => x.IsHighPerformer == hiperf);
        }

        if (request.PerformanceBand is { } perfBand)
        {
            query = query.Where(x => x.PerformanceBand == perfBand);
        }

        if (request.PotentialBand is { } potBand)
        {
            query = query.Where(x => x.PotentialBand == potBand);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.ClassifiedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TalentClassificationDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                PerformanceCycleId = x.PerformanceCycleId,
                TalentScoreId = x.TalentScoreId,
                PerformanceBand = x.PerformanceBand,
                PotentialBand = x.PotentialBand,
                NineBoxCode = x.NineBoxCode,
                CategoryName = x.CategoryName,
                IsHighPotential = x.IsHighPotential,
                IsHighPerformer = x.IsHighPerformer,
                ClassifiedOnUtc = x.ClassifiedOnUtc,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<TalentClassificationDto>>.Ok(new PagedResult<TalentClassificationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private async Task<(decimal LowThreshold, decimal HighThreshold)> GetActiveThresholdsAsync(
        CancellationToken cancellationToken)
    {
        var active = await _db.ClassificationRuleSets.AsNoTracking()
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .OrderByDescending(x => x.EffectiveFromUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (active is not null)
        {
            return (active.LowThreshold, active.HighThreshold);
        }

        return (ClassificationConstants.DefaultLowThreshold, ClassificationConstants.DefaultHighThreshold);
    }

    private static ClassificationComputation ComputeClassification(
        TalentScore talentScore,
        decimal lowThreshold,
        decimal highThreshold)
    {
        var performanceBand = TalentNineBoxMatrix.ResolvePerformanceBand(
            talentScore.PerformanceScore,
            lowThreshold,
            highThreshold);

        var potentialBand = TalentNineBoxMatrix.ResolvePotentialBand(
            talentScore.PotentialScore,
            lowThreshold,
            highThreshold);

        var (nineBoxCode, categoryName) = TalentNineBoxMatrix.Resolve(performanceBand, potentialBand);

        return new ClassificationComputation(
            performanceBand,
            potentialBand,
            nineBoxCode,
            categoryName,
            potentialBand == PotentialBand.High,
            performanceBand == PerformanceBand.High);
    }

    private static bool CycleAllowsClassification(PerformanceCycleStatus status) =>
        status is PerformanceCycleStatus.Draft
            or PerformanceCycleStatus.Active
            or PerformanceCycleStatus.Closed;

    private static TalentClassificationDto MapToDto(TalentClassification entity) =>
        new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PerformanceCycleId = entity.PerformanceCycleId,
            TalentScoreId = entity.TalentScoreId,
            PerformanceBand = entity.PerformanceBand,
            PotentialBand = entity.PotentialBand,
            NineBoxCode = entity.NineBoxCode,
            CategoryName = entity.CategoryName,
            IsHighPotential = entity.IsHighPotential,
            IsHighPerformer = entity.IsHighPerformer,
            ClassifiedOnUtc = entity.ClassifiedOnUtc,
            Notes = entity.Notes
        };

    private sealed record ClassificationComputation(
        PerformanceBand PerformanceBand,
        PotentialBand PotentialBand,
        NineBoxCode NineBoxCode,
        string CategoryName,
        bool IsHighPotential,
        bool IsHighPerformer);
}
