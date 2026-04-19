using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Application.Features.Competencies.Interfaces;
using TalentSystem.Domain.Competencies;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Competencies.Services;

public sealed class CompetencyCategoryService : ICompetencyCategoryService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateCompetencyCategoryRequest> _createValidator;
    private readonly IValidator<UpdateCompetencyCategoryRequest> _updateValidator;
    private readonly IValidator<CompetencyCategoryFilterRequest> _filterValidator;

    public CompetencyCategoryService(
        TalentDbContext db,
        IValidator<CreateCompetencyCategoryRequest> createValidator,
        IValidator<UpdateCompetencyCategoryRequest> updateValidator,
        IValidator<CompetencyCategoryFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<CompetencyCategoryDto>> CreateAsync(
        CreateCompetencyCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyCategoryDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = new CompetencyCategory
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _db.CompetencyCategories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyCategoryDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyCategoryDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyCategoryDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.CompetencyCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CompetencyCategoryDto>.Fail(
                "The competency category was not found.",
                CompetencyErrors.CategoryNotFound);
        }

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyCategoryDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyCategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CompetencyCategories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<CompetencyCategoryDto>.Fail(
                "The competency category was not found.",
                CompetencyErrors.CategoryNotFound);
        }

        return Result<CompetencyCategoryDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<CompetencyCategoryDto>>> GetPagedAsync(
        CompetencyCategoryFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<CompetencyCategoryDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<CompetencyCategory> query = _db.CompetencyCategories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.NameEn.Contains(term) ||
                x.NameAr.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.NameEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompetencyCategoryDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CompetencyCategoryDto>>.Ok(new PagedResult<CompetencyCategoryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static CompetencyCategoryDto MapToDto(CompetencyCategory entity) =>
        new()
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description
        };
}
