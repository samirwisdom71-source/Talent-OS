using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;
using Result = TalentSystem.Shared.Results.Result;

namespace TalentSystem.Application.Features.Analytics.Services;

public sealed class TalentAnalyticsService : ITalentAnalyticsService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<TalentAnalyticsFilterRequest> _filterValidator;

    public TalentAnalyticsService(
        TalentDbContext db,
        IValidator<TalentAnalyticsFilterRequest> filterValidator)
    {
        _db = db;
        _filterValidator = filterValidator;
    }

    public async Task<Result<TalentDistributionSummaryDto>> GetDistributionAsync(
        TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<TalentDistributionSummaryDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var refValidation = await ValidateFilterReferencesAsync(filter, cancellationToken).ConfigureAwait(false);
        if (refValidation.IsFailure)
        {
            return Result<TalentDistributionSummaryDto>.Fail(refValidation.Errors, refValidation.FailureCode);
        }

        var baseQuery = FilteredClassifications(filter);

        var nineBox = await baseQuery
            .GroupBy(tc => tc.NineBoxCode)
            .Select(g => new NineBoxDistributionItemDto { NineBoxCode = g.Key, Count = g.Count() })
            .OrderBy(x => x.NineBoxCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var perfBands = await baseQuery
            .GroupBy(tc => tc.PerformanceBand)
            .Select(g => new EnumCountDto<PerformanceBand> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var potBands = await baseQuery
            .GroupBy(tc => tc.PotentialBand)
            .Select(g => new EnumCountDto<PotentialBand> { Value = g.Key, Count = g.Count() })
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var categories = await baseQuery
            .GroupBy(tc => tc.CategoryName)
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<TalentDistributionSummaryDto>.Ok(new TalentDistributionSummaryDto
        {
            ByNineBox = nineBox,
            ByPerformanceBand = perfBands,
            ByPotentialBand = potBands,
            ByCategoryName = categories
        });
    }

    public async Task<Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>> GetByCycleAsync(
        TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var refValidation = await ValidateFilterReferencesAsync(filter, cancellationToken).ConfigureAwait(false);
        if (refValidation.IsFailure)
        {
            return Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>.Fail(
                refValidation.Errors,
                refValidation.FailureCode);
        }

        var baseQuery = FilteredClassifications(filter);

        var grouped = await baseQuery
            .GroupBy(tc => tc.PerformanceCycleId)
            .Select(g => new { PerformanceCycleId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (grouped.Count == 0)
        {
            return Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>.Ok(Array.Empty<TalentClassificationByCycleSummaryDto>());
        }

        var cycleIds = grouped.Select(x => x.PerformanceCycleId).ToList();
        var names = await _db.PerformanceCycles.AsNoTracking()
            .Where(c => cycleIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NameEn, cancellationToken)
            .ConfigureAwait(false);

        var list = grouped
            .Select(g => new TalentClassificationByCycleSummaryDto
            {
                PerformanceCycleId = g.PerformanceCycleId,
                PerformanceCycleNameEn = names.GetValueOrDefault(g.PerformanceCycleId, string.Empty),
                ClassificationCount = g.Count
            })
            .ToList();

        return Result<IReadOnlyList<TalentClassificationByCycleSummaryDto>>.Ok(list);
    }

    private async Task<Result> ValidateFilterReferencesAsync(
        TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken)
    {
        if (filter.PerformanceCycleId is { } cycleId)
        {
            var exists = await _db.PerformanceCycles.AsNoTracking()
                .AnyAsync(c => c.Id == cycleId, cancellationToken)
                .ConfigureAwait(false);
            if (!exists)
            {
                return Result.Fail("Performance cycle was not found.", PerformanceErrors.CycleNotFound);
            }
        }

        if (filter.OrganizationUnitId is { } ouId)
        {
            var exists = await _db.OrganizationUnits.AsNoTracking()
                .AnyAsync(o => o.Id == ouId, cancellationToken)
                .ConfigureAwait(false);
            if (!exists)
            {
                return Result.Fail("Organization unit was not found.", EmployeeErrors.OrganizationUnitNotFound);
            }
        }

        return Result.Ok();
    }

    private IQueryable<TalentClassification> FilteredClassifications(TalentAnalyticsFilterRequest filter)
    {
        var q = _db.TalentClassifications.AsNoTracking();

        if (filter.PerformanceCycleId is { } cycleId)
        {
            q = q.Where(tc => tc.PerformanceCycleId == cycleId);
        }

        if (filter.OrganizationUnitId is { } ouId)
        {
            q = q.Where(tc =>
                _db.Employees.Any(e => e.Id == tc.EmployeeId && e.OrganizationUnitId == ouId));
        }

        return q;
    }
}
