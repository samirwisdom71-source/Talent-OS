using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Application.Features.Classification.Interfaces;
using TalentSystem.Domain.Classification;
using TalentSystem.Domain.Enums;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Classification.Services;

public sealed class ClassificationRuleSetService : IClassificationRuleSetService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateClassificationRuleSetRequest> _createValidator;
    private readonly IValidator<UpdateClassificationRuleSetRequest> _updateValidator;
    private readonly IValidator<ClassificationRuleSetFilterRequest> _filterValidator;

    public ClassificationRuleSetService(
        TalentDbContext db,
        IValidator<CreateClassificationRuleSetRequest> createValidator,
        IValidator<UpdateClassificationRuleSetRequest> updateValidator,
        IValidator<ClassificationRuleSetFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<ClassificationRuleSetDto>> CreateAsync(
        CreateClassificationRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ClassificationRuleSetDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var normalizedVersion = request.Version.Trim();
        if (await _db.ClassificationRuleSets.AsNoTracking().AnyAsync(
                x => x.Version == normalizedVersion,
                cancellationToken))
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "A classification rule set with this version already exists.",
                ClassificationErrors.RuleSetVersionDuplicate);
        }

        var entity = new ClassificationRuleSet
        {
            Name = request.Name.Trim(),
            Version = normalizedVersion,
            LowThreshold = request.LowThreshold,
            HighThreshold = request.HighThreshold,
            EffectiveFromUtc = request.EffectiveFromUtc,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordStatus = RecordStatus.Archived
        };

        _db.ClassificationRuleSets.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<ClassificationRuleSetDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<ClassificationRuleSetDto>> UpdateAsync(
        Guid id,
        UpdateClassificationRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ClassificationRuleSetDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.ClassificationRuleSets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "The classification rule set was not found.",
                ClassificationErrors.RuleSetNotFound);
        }

        var normalizedVersion = request.Version.Trim();
        if (!string.Equals(entity.Version, normalizedVersion, StringComparison.Ordinal) &&
            await _db.ClassificationRuleSets.AsNoTracking().AnyAsync(
                x => x.Version == normalizedVersion && x.Id != id,
                cancellationToken))
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "A classification rule set with this version already exists.",
                ClassificationErrors.RuleSetVersionDuplicate);
        }

        entity.Name = request.Name.Trim();
        entity.Version = normalizedVersion;
        entity.LowThreshold = request.LowThreshold;
        entity.HighThreshold = request.HighThreshold;
        entity.EffectiveFromUtc = request.EffectiveFromUtc;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<ClassificationRuleSetDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<ClassificationRuleSetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ClassificationRuleSets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "The classification rule set was not found.",
                ClassificationErrors.RuleSetNotFound);
        }

        return Result<ClassificationRuleSetDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<ClassificationRuleSetDto>>> GetPagedAsync(
        ClassificationRuleSetFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<ClassificationRuleSetDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<ClassificationRuleSet> query = _db.ClassificationRuleSets.AsNoTracking();

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
            .Select(x => new ClassificationRuleSetDto
            {
                Id = x.Id,
                Name = x.Name,
                Version = x.Version,
                LowThreshold = x.LowThreshold,
                HighThreshold = x.HighThreshold,
                EffectiveFromUtc = x.EffectiveFromUtc,
                Notes = x.Notes,
                RecordStatus = x.RecordStatus
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ClassificationRuleSetDto>>.Ok(new PagedResult<ClassificationRuleSetDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<ClassificationRuleSetDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ClassificationRuleSets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "The classification rule set was not found.",
                ClassificationErrors.RuleSetNotFound);
        }

        if (entity.RecordStatus == RecordStatus.Active)
        {
            return Result<ClassificationRuleSetDto>.Ok(MapToDto(entity));
        }

        if (entity.RecordStatus != RecordStatus.Archived)
        {
            return Result<ClassificationRuleSetDto>.Fail(
                "Only an archived classification rule set can be activated.",
                ClassificationErrors.RuleSetMustBeInactiveToActivate);
        }

        var currentlyActive = await _db.ClassificationRuleSets
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var active in currentlyActive)
        {
            active.RecordStatus = RecordStatus.Archived;
        }

        entity.RecordStatus = RecordStatus.Active;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<ClassificationRuleSetDto>.Ok(MapToDto(entity));
    }

    private static ClassificationRuleSetDto MapToDto(ClassificationRuleSet entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Version = entity.Version,
            LowThreshold = entity.LowThreshold,
            HighThreshold = entity.HighThreshold,
            EffectiveFromUtc = entity.EffectiveFromUtc,
            Notes = entity.Notes,
            RecordStatus = entity.RecordStatus
        };
}
