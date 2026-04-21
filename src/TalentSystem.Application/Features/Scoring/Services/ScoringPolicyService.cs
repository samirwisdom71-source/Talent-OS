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

public sealed class ScoringPolicyService : IScoringPolicyService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateScoringPolicyRequest> _createValidator;
    private readonly IValidator<UpdateScoringPolicyRequest> _updateValidator;
    private readonly IValidator<ScoringPolicyFilterRequest> _filterValidator;

    public ScoringPolicyService(
        TalentDbContext db,
        IValidator<CreateScoringPolicyRequest> createValidator,
        IValidator<UpdateScoringPolicyRequest> updateValidator,
        IValidator<ScoringPolicyFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<ScoringPolicyDto>> CreateAsync(
        CreateScoringPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ScoringPolicyDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var normalizedVersion = request.Version.Trim();
        if (await _db.ScoringPolicies.AsNoTracking().AnyAsync(
                x => x.Version == normalizedVersion,
                cancellationToken))
        {
            return Result<ScoringPolicyDto>.Fail(
                "A scoring policy with this version already exists.",
                ScoringErrors.PolicyVersionDuplicate);
        }

        var entity = new ScoringPolicy
        {
            Name = request.Name.Trim(),
            Version = normalizedVersion,
            PerformanceWeight = request.PerformanceWeight,
            PotentialWeight = request.PotentialWeight,
            EffectiveFromUtc = request.EffectiveFromUtc,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordStatus = RecordStatus.Archived
        };

        _db.ScoringPolicies.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<ScoringPolicyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<ScoringPolicyDto>> UpdateAsync(
        Guid id,
        UpdateScoringPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ScoringPolicyDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.ScoringPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<ScoringPolicyDto>.Fail(
                "The scoring policy was not found.",
                ScoringErrors.PolicyNotFound);
        }

        var normalizedVersion = request.Version.Trim();
        if (!string.Equals(entity.Version, normalizedVersion, StringComparison.Ordinal) &&
            await _db.ScoringPolicies.AsNoTracking().AnyAsync(
                x => x.Version == normalizedVersion && x.Id != id,
                cancellationToken))
        {
            return Result<ScoringPolicyDto>.Fail(
                "A scoring policy with this version already exists.",
                ScoringErrors.PolicyVersionDuplicate);
        }

        entity.Name = request.Name.Trim();
        entity.Version = normalizedVersion;
        entity.PerformanceWeight = request.PerformanceWeight;
        entity.PotentialWeight = request.PotentialWeight;
        entity.EffectiveFromUtc = request.EffectiveFromUtc;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<ScoringPolicyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<ScoringPolicyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ScoringPolicies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<ScoringPolicyDto>.Fail(
                "The scoring policy was not found.",
                ScoringErrors.PolicyNotFound);
        }

        return Result<ScoringPolicyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<ScoringPolicyDto>>> GetPagedAsync(
        ScoringPolicyFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<ScoringPolicyDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<ScoringPolicy> query = _db.ScoringPolicies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Version.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.EffectiveFromUtc)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ScoringPolicyDto
            {
                Id = x.Id,
                Name = x.Name,
                Version = x.Version,
                PerformanceWeight = x.PerformanceWeight,
                PotentialWeight = x.PotentialWeight,
                EffectiveFromUtc = x.EffectiveFromUtc,
                Notes = x.Notes,
                RecordStatus = x.RecordStatus
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ScoringPolicyDto>>.Ok(new PagedResult<ScoringPolicyDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<ScoringPolicyDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var policy = await _db.ScoringPolicies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (policy is null)
        {
            return Result<ScoringPolicyDto>.Fail(
                "The scoring policy was not found.",
                ScoringErrors.PolicyNotFound);
        }

        if (policy.RecordStatus == RecordStatus.Active)
        {
            return Result<ScoringPolicyDto>.Ok(MapToDto(policy));
        }

        if (policy.RecordStatus != RecordStatus.Archived)
        {
            return Result<ScoringPolicyDto>.Fail(
                "Only an archived scoring policy can be activated.",
                ScoringErrors.PolicyMustBeInactiveToActivate);
        }

        var currentlyActive = await _db.ScoringPolicies
            .Where(x => x.RecordStatus == RecordStatus.Active && x.Id != policy.Id)
            .ToListAsync(cancellationToken);

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            if (currentlyActive.Count > 0)
            {
                foreach (var active in currentlyActive)
                {
                    active.RecordStatus = RecordStatus.Archived;
                }

                await _db.SaveChangesAsync(cancellationToken);
            }

            policy.RecordStatus = RecordStatus.Active;
            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        return Result<ScoringPolicyDto>.Ok(MapToDto(policy));
    }

    private static ScoringPolicyDto MapToDto(ScoringPolicy entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Version = entity.Version,
            PerformanceWeight = entity.PerformanceWeight,
            PotentialWeight = entity.PotentialWeight,
            EffectiveFromUtc = entity.EffectiveFromUtc,
            Notes = entity.Notes,
            RecordStatus = entity.RecordStatus
        };
}
