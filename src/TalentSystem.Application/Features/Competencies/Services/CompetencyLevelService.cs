using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Application.Features.Competencies.Interfaces;
using TalentSystem.Domain.Competencies;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Services;

public sealed class CompetencyLevelService : ICompetencyLevelService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateCompetencyLevelRequest> _createValidator;
    private readonly IValidator<UpdateCompetencyLevelRequest> _updateValidator;
    private readonly IValidator<CompetencyLevelFilterRequest> _filterValidator;

    public CompetencyLevelService(
        TalentDbContext db,
        IValidator<CreateCompetencyLevelRequest> createValidator,
        IValidator<UpdateCompetencyLevelRequest> updateValidator,
        IValidator<CompetencyLevelFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<CompetencyLevelDto>> CreateAsync(
        CreateCompetencyLevelRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyLevelDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (await _db.CompetencyLevels.AsNoTracking().AnyAsync(
                x => x.NumericValue == request.NumericValue,
                cancellationToken))
        {
            return Result<CompetencyLevelDto>.Fail(
                "A competency level with this numeric value already exists.",
                CompetencyErrors.DuplicateCompetencyLevelNumericValue);
        }

        var entity = new CompetencyLevel
        {
            Name = request.Name.Trim(),
            NumericValue = request.NumericValue,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _db.CompetencyLevels.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyLevelDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyLevelDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyLevelRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyLevelDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.CompetencyLevels.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CompetencyLevelDto>.Fail(
                "The competency level was not found.",
                CompetencyErrors.LevelNotFound);
        }

        if (await _db.CompetencyLevels.AsNoTracking().AnyAsync(
                x => x.NumericValue == request.NumericValue && x.Id != id,
                cancellationToken))
        {
            return Result<CompetencyLevelDto>.Fail(
                "A competency level with this numeric value already exists.",
                CompetencyErrors.DuplicateCompetencyLevelNumericValue);
        }

        entity.Name = request.Name.Trim();
        entity.NumericValue = request.NumericValue;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyLevelDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyLevelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CompetencyLevels.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<CompetencyLevelDto>.Fail(
                "The competency level was not found.",
                CompetencyErrors.LevelNotFound);
        }

        return Result<CompetencyLevelDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<CompetencyLevelDto>>> GetPagedAsync(
        CompetencyLevelFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<CompetencyLevelDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<CompetencyLevel> query = _db.CompetencyLevels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.NumericValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompetencyLevelDto
            {
                Id = x.Id,
                Name = x.Name,
                NumericValue = x.NumericValue,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CompetencyLevelDto>>.Ok(new PagedResult<CompetencyLevelDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetLookupAsync(
        CompetencyLevelLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        var take = request.Take <= 0 ? PaginationConstants.MaxPageSize : request.Take;
        if (take > PaginationConstants.MaxPageSize)
        {
            take = PaginationConstants.MaxPageSize;
        }

        IQueryable<CompetencyLevel> query = _db.CompetencyLevels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var rows = await query
            .OrderBy(x => x.NumericValue)
            .Take(take)
            .Select(x => new { x.Id, x.Name, x.NumericValue })
            .ToListAsync(cancellationToken);

        var list = rows
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = $"{x.Name.Trim()} (L{x.NumericValue})",
                Email = null
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(list);
    }

    private static CompetencyLevelDto MapToDto(CompetencyLevel entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            NumericValue = entity.NumericValue,
            Description = entity.Description
        };
}
