using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Intelligence.DTOs;
using TalentSystem.Application.Features.Intelligence.Interfaces;
using TalentSystem.Application.Features.Intelligence.Models;
using TalentSystem.Domain.Development;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;
using TalentSystem.Domain.Performance;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Intelligence.Services;

public sealed class IntelligenceService : IIntelligenceService
{
    private readonly ILogger<IntelligenceService> _logger;
    private readonly TalentDbContext _db;
    private readonly IIntelligenceProvider _provider;
    private readonly IValidator<GenerateEmployeeIntelligenceRequest> _employeeGenValidator;
    private readonly IValidator<GenerateCycleIntelligenceRequest> _cycleGenValidator;
    private readonly IValidator<TalentInsightFilterRequest> _insightFilterValidator;
    private readonly IValidator<TalentRecommendationFilterRequest> _recommendationFilterValidator;

    public IntelligenceService(
        ILogger<IntelligenceService> logger,
        TalentDbContext db,
        IIntelligenceProvider provider,
        IValidator<GenerateEmployeeIntelligenceRequest> employeeGenValidator,
        IValidator<GenerateCycleIntelligenceRequest> cycleGenValidator,
        IValidator<TalentInsightFilterRequest> insightFilterValidator,
        IValidator<TalentRecommendationFilterRequest> recommendationFilterValidator)
    {
        _logger = logger;
        _db = db;
        _provider = provider;
        _employeeGenValidator = employeeGenValidator;
        _cycleGenValidator = cycleGenValidator;
        _insightFilterValidator = insightFilterValidator;
        _recommendationFilterValidator = recommendationFilterValidator;
    }

    public Task<Result<IntelligenceGenerationResultDto>> GenerateInsightsForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default) =>
        GenerateForEmployeeCoreAsync(request, IntelligenceScope.InsightsOnly, cancellationToken, skipRun: false);

    public Task<Result<IntelligenceGenerationResultDto>> GenerateRecommendationsForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default) =>
        GenerateForEmployeeCoreAsync(request, IntelligenceScope.RecommendationsOnly, cancellationToken, skipRun: false);

    public Task<Result<IntelligenceGenerationResultDto>> GenerateAllForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default) =>
        GenerateForEmployeeCoreAsync(request, IntelligenceScope.All, cancellationToken, skipRun: false);

    public async Task<Result<IntelligenceGenerationResultDto>> GenerateForPerformanceCycleAsync(
        GenerateCycleIntelligenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _cycleGenValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.PerformanceCycleId, cancellationToken)
            .ConfigureAwait(false);
        if (cycle is null)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (cycle.Status == PerformanceCycleStatus.Archived)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(
                "Intelligence generation is not allowed for an archived performance cycle.",
                IntelligenceErrors.CycleArchived);
        }

        var employeeIds = await LoadDistinctEmployeeIdsForCycleAsync(request.PerformanceCycleId, cancellationToken)
            .ConfigureAwait(false);

        var run = new IntelligenceRun
        {
            RunType = IntelligenceRunType.Cycle,
            PerformanceCycleId = request.PerformanceCycleId,
            EmployeeId = null,
            StartedOnUtc = DateTime.UtcNow,
            Status = IntelligenceRunStatus.Started,
            TotalInsightsGenerated = 0,
            TotalRecommendationsGenerated = 0,
            Notes = $"scope:{IntelligenceScope.All};employees:{employeeIds.Count}"
        };

        _db.IntelligenceRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var totalInsights = 0;
        var totalRecommendations = 0;

        try
        {
            foreach (var employeeId in employeeIds)
            {
                var inner = await GenerateForEmployeeCoreAsync(
                        new GenerateEmployeeIntelligenceRequest
                        {
                            EmployeeId = employeeId,
                            PerformanceCycleId = request.PerformanceCycleId
                        },
                        IntelligenceScope.All,
                        cancellationToken,
                        skipRun: true)
                    .ConfigureAwait(false);

                if (inner.IsFailure)
                {
                    throw new InvalidOperationException(string.Join(" · ", inner.Errors));
                }

                totalInsights += inner.Value!.InsightsGenerated;
                totalRecommendations += inner.Value.RecommendationsGenerated;
            }

            run.Status = IntelligenceRunStatus.Completed;
            run.CompletedOnUtc = DateTime.UtcNow;
            run.TotalInsightsGenerated = totalInsights;
            run.TotalRecommendationsGenerated = totalRecommendations;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Intelligence cycle run {RunId} completed for performance cycle {CycleId}: insights={Insights}, recommendations={Recommendations}.",
                run.Id,
                request.PerformanceCycleId,
                totalInsights,
                totalRecommendations);

            return Result<IntelligenceGenerationResultDto>.Ok(new IntelligenceGenerationResultDto
            {
                RunId = run.Id,
                InsightsGenerated = totalInsights,
                RecommendationsGenerated = totalRecommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Intelligence cycle run {RunId} failed for performance cycle {CycleId}.",
                run.Id,
                request.PerformanceCycleId);
            run.Status = IntelligenceRunStatus.Failed;
            run.CompletedOnUtc = DateTime.UtcNow;
            run.TotalInsightsGenerated = totalInsights;
            run.TotalRecommendationsGenerated = totalRecommendations;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<Result<TalentInsightDto>> GetInsightByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.TalentInsights.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return Result<TalentInsightDto>.Fail("The insight was not found.", IntelligenceErrors.InsightNotFound);
        }

        return MapInsight(row);
    }

    public async Task<Result<TalentRecommendationDto>> GetRecommendationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.TalentRecommendations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return Result<TalentRecommendationDto>.Fail(
                "The recommendation was not found.",
                IntelligenceErrors.RecommendationNotFound);
        }

        return MapRecommendation(row);
    }

    public async Task<Result<PagedResult<TalentInsightDto>>> GetPagedInsightsAsync(
        TalentInsightFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _insightFilterValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<PagedResult<TalentInsightDto>>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var query = _db.TalentInsights.AsNoTracking().AsQueryable();
        if (request.EmployeeId is { } eid)
        {
            query = query.Where(x => x.EmployeeId == eid);
        }

        if (request.PerformanceCycleId is { } cid)
        {
            query = query.Where(x => x.PerformanceCycleId == cid);
        }

        if (request.Status is { } st)
        {
            query = query.Where(x => x.Status == st);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(x => x.GeneratedOnUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<TalentInsightDto>>.Ok(new PagedResult<TalentInsightDto>
        {
            Items = items.Select(MapInsight).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        });
    }

    public async Task<Result<PagedResult<TalentRecommendationDto>>> GetPagedRecommendationsAsync(
        TalentRecommendationFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _recommendationFilterValidator.ValidateAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<PagedResult<TalentRecommendationDto>>.Fail(
                validation.Errors.Select(x => x.ErrorMessage).ToList());
        }

        var query = _db.TalentRecommendations.AsNoTracking().AsQueryable();
        if (request.EmployeeId is { } eid)
        {
            query = query.Where(x => x.EmployeeId == eid);
        }

        if (request.PerformanceCycleId is { } cid)
        {
            query = query.Where(x => x.PerformanceCycleId == cid);
        }

        if (request.Status is { } st)
        {
            query = query.Where(x => x.Status == st);
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(x => x.GeneratedOnUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<TalentRecommendationDto>>.Ok(new PagedResult<TalentRecommendationDto>
        {
            Items = items.Select(MapRecommendation).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        });
    }

    public async Task<Result> DismissInsightAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.TalentInsights.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return Result.Fail("The insight was not found.", IntelligenceErrors.InsightNotFound);
        }

        if (row.Status != TalentInsightStatus.Active)
        {
            return Result.Fail("Only active insights can be dismissed.", IntelligenceErrors.InvalidInsightStatus);
        }

        row.Status = TalentInsightStatus.Dismissed;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result> DismissRecommendationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.TalentRecommendations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return Result.Fail("The recommendation was not found.", IntelligenceErrors.RecommendationNotFound);
        }

        if (row.Status != TalentRecommendationStatus.Active)
        {
            return Result.Fail(
                "Only active recommendations can be dismissed.",
                IntelligenceErrors.InvalidRecommendationStatus);
        }

        row.Status = TalentRecommendationStatus.Dismissed;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result> AcceptRecommendationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.TalentRecommendations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return Result.Fail("The recommendation was not found.", IntelligenceErrors.RecommendationNotFound);
        }

        if (row.Status != TalentRecommendationStatus.Active)
        {
            return Result.Fail(
                "Only active recommendations can be accepted.",
                IntelligenceErrors.InvalidRecommendationStatus);
        }

        row.Status = TalentRecommendationStatus.Accepted;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    private async Task<Result<IntelligenceGenerationResultDto>> GenerateForEmployeeCoreAsync(
        GenerateEmployeeIntelligenceRequest request,
        IntelligenceScope scope,
        CancellationToken cancellationToken,
        bool skipRun)
    {
        var validation = await _employeeGenValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Employees.AsNoTracking().AnyAsync(e => e.Id == request.EmployeeId, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result<IntelligenceGenerationResultDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var cycle = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.PerformanceCycleId, cancellationToken)
            .ConfigureAwait(false);
        if (cycle is null)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (cycle.Status == PerformanceCycleStatus.Archived)
        {
            return Result<IntelligenceGenerationResultDto>.Fail(
                "Intelligence generation is not allowed for an archived performance cycle.",
                IntelligenceErrors.CycleArchived);
        }

        var context = await BuildContextAsync(request.EmployeeId, request.PerformanceCycleId, cancellationToken)
            .ConfigureAwait(false);

        var insightCount = 0;
        var recommendationCount = 0;
        IntelligenceRun? run = null;

        // SqlServerRetryingExecutionStrategy + explicit transactions: wrap in CreateExecutionStrategy().
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            insightCount = 0;
            recommendationCount = 0;
            run = null;

            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!skipRun)
                {
                    run = new IntelligenceRun
                    {
                        RunType = IntelligenceRunType.Employee,
                        PerformanceCycleId = request.PerformanceCycleId,
                        EmployeeId = request.EmployeeId,
                        StartedOnUtc = DateTime.UtcNow,
                        Status = IntelligenceRunStatus.Started,
                        Notes = $"scope:{scope}"
                    };
                    _db.IntelligenceRuns.Add(run);
                }

                if (scope == IntelligenceScope.InsightsOnly || scope == IntelligenceScope.All)
                {
                    await ArchiveRulesEngineActiveInsightsAsync(
                            request.EmployeeId,
                            request.PerformanceCycleId,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                if (scope == IntelligenceScope.RecommendationsOnly || scope == IntelligenceScope.All)
                {
                    await ArchiveRulesEngineActiveRecommendationsAsync(
                            request.EmployeeId,
                            request.PerformanceCycleId,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                if (context is not null)
                {
                    if (scope == IntelligenceScope.InsightsOnly || scope == IntelligenceScope.All)
                    {
                        var insights = _provider.BuildInsights(context);
                        foreach (var insight in insights)
                        {
                            _db.TalentInsights.Add(insight);
                        }

                        insightCount = insights.Count;
                    }

                    if (scope == IntelligenceScope.RecommendationsOnly || scope == IntelligenceScope.All)
                    {
                        var recs = _provider.BuildRecommendations(context);
                        foreach (var rec in recs)
                        {
                            _db.TalentRecommendations.Add(rec);
                        }

                        recommendationCount = recs.Count;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (run is not null)
                {
                    run.Status = IntelligenceRunStatus.Completed;
                    run.CompletedOnUtc = DateTime.UtcNow;
                    run.TotalInsightsGenerated = insightCount;
                    run.TotalRecommendationsGenerated = recommendationCount;
                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Intelligence employee run failed for employee {EmployeeId}, cycle {CycleId}, scope {Scope}.",
                    request.EmployeeId,
                    request.PerformanceCycleId,
                    scope);
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                if (run is not null)
                {
                    run.Status = IntelligenceRunStatus.Failed;
                    run.CompletedOnUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                throw;
            }
        }).ConfigureAwait(false);

        if (insightCount > 0 || recommendationCount > 0)
        {
            _logger.LogInformation(
                "Intelligence generated for employee {EmployeeId}, cycle {CycleId}: insights={Insights}, recommendations={Recommendations}, run {RunId}.",
                request.EmployeeId,
                request.PerformanceCycleId,
                insightCount,
                recommendationCount,
                run?.Id);
        }

        return Result<IntelligenceGenerationResultDto>.Ok(new IntelligenceGenerationResultDto
        {
            RunId = run?.Id,
            InsightsGenerated = insightCount,
            RecommendationsGenerated = recommendationCount
        });
    }

    private async Task<List<Guid>> LoadDistinctEmployeeIdsForCycleAsync(
        Guid cycleId,
        CancellationToken cancellationToken)
    {
        var fromScores = await _db.TalentScores.AsNoTracking()
            .Where(s => s.PerformanceCycleId == cycleId)
            .Select(s => s.EmployeeId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var fromClass = await _db.TalentClassifications.AsNoTracking()
            .Where(c => c.PerformanceCycleId == cycleId)
            .Select(c => c.EmployeeId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return fromScores.Union(fromClass).Distinct().ToList();
    }

    private async Task ArchiveRulesEngineActiveInsightsAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken)
    {
        var rows = await _db.TalentInsights
            .Where(x =>
                x.EmployeeId == employeeId
                && x.PerformanceCycleId == performanceCycleId
                && x.Source == InsightSource.RulesEngine
                && x.Status == TalentInsightStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in rows)
        {
            row.Status = TalentInsightStatus.Archived;
        }
    }

    private async Task ArchiveRulesEngineActiveRecommendationsAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken)
    {
        var rows = await _db.TalentRecommendations
            .Where(x =>
                x.EmployeeId == employeeId
                && x.PerformanceCycleId == performanceCycleId
                && x.Source == RecommendationSource.RulesEngine
                && x.Status == TalentRecommendationStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in rows)
        {
            row.Status = TalentRecommendationStatus.Archived;
        }
    }

    private async Task<EmployeeIntelligenceContext?> BuildContextAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken)
    {
        var classification = await _db.TalentClassifications.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.EmployeeId == employeeId && c.PerformanceCycleId == performanceCycleId,
                cancellationToken)
            .ConfigureAwait(false);

        var score = await _db.TalentScores.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.EmployeeId == employeeId && s.PerformanceCycleId == performanceCycleId,
                cancellationToken)
            .ConfigureAwait(false);

        if (classification is null && score is null)
        {
            return null;
        }

        var isPrimarySuccessor = await (
                from c in _db.SuccessorCandidates.AsNoTracking()
                join p in _db.SuccessionPlans.AsNoTracking() on c.SuccessionPlanId equals p.Id
                where c.EmployeeId == employeeId
                    && c.IsPrimarySuccessor
                    && c.RecordStatus == RecordStatus.Active
                    && p.RecordStatus == RecordStatus.Active
                    && p.Status == SuccessionPlanStatus.Active
                select c.Id)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasActivePlan = await _db.DevelopmentPlans.AsNoTracking()
            .AnyAsync(
                p => p.EmployeeId == employeeId
                    && p.PerformanceCycleId == performanceCycleId
                    && p.Status == DevelopmentPlanStatus.Active,
                cancellationToken)
            .ConfigureAwait(false);

        return new EmployeeIntelligenceContext
        {
            EmployeeId = employeeId,
            PerformanceCycleId = performanceCycleId,
            HasClassification = classification is not null,
            PerformanceBand = classification?.PerformanceBand,
            PotentialBand = classification?.PotentialBand,
            NineBoxCode = classification?.NineBoxCode,
            IsHighPotential = classification?.IsHighPotential ?? false,
            IsHighPerformer = classification?.IsHighPerformer ?? false,
            PerformanceScore = score?.PerformanceScore,
            PotentialScore = score?.PotentialScore,
            FinalScore = score?.FinalScore,
            IsPrimarySuccessor = isPrimarySuccessor,
            HasActiveDevelopmentPlan = hasActivePlan
        };
    }

    private static TalentInsightDto MapInsight(TalentInsight x) =>
        new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            PerformanceCycleId = x.PerformanceCycleId,
            InsightType = x.InsightType,
            Severity = x.Severity,
            Source = x.Source,
            Title = x.Title,
            Summary = x.Summary,
            ConfidenceScore = x.ConfidenceScore,
            RelatedEntityId = x.RelatedEntityId,
            RelatedEntityType = x.RelatedEntityType,
            Status = x.Status,
            GeneratedOnUtc = x.GeneratedOnUtc,
            Notes = x.Notes
        };

    private static TalentRecommendationDto MapRecommendation(TalentRecommendation x) =>
        new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            PerformanceCycleId = x.PerformanceCycleId,
            RecommendationType = x.RecommendationType,
            Priority = x.Priority,
            Source = x.Source,
            Title = x.Title,
            Description = x.Description,
            RecommendedAction = x.RecommendedAction,
            ConfidenceScore = x.ConfidenceScore,
            RelatedEntityId = x.RelatedEntityId,
            RelatedEntityType = x.RelatedEntityType,
            Status = x.Status,
            GeneratedOnUtc = x.GeneratedOnUtc,
            Notes = x.Notes
        };

    private enum IntelligenceScope
    {
        InsightsOnly,
        RecommendationsOnly,
        All
    }
}
