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

public sealed class CompetencyService : ICompetencyService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateCompetencyRequest> _createValidator;
    private readonly IValidator<UpdateCompetencyRequest> _updateValidator;
    private readonly IValidator<CompetencyFilterRequest> _filterValidator;

    public CompetencyService(
        TalentDbContext db,
        IValidator<CreateCompetencyRequest> createValidator,
        IValidator<UpdateCompetencyRequest> updateValidator,
        IValidator<CompetencyFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<CompetencyDto>> CreateAsync(
        CreateCompetencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var categoryExists = await _db.CompetencyCategories.AsNoTracking()
            .AnyAsync(x => x.Id == request.CompetencyCategoryId, cancellationToken);
        if (!categoryExists)
        {
            return Result<CompetencyDto>.Fail(
                "The competency category was not found.",
                CompetencyErrors.CompetencyCategoryMissing);
        }

        var normalizedCode = request.Code.Trim();
        if (await _db.Competencies.AsNoTracking().AnyAsync(
                x => x.Code == normalizedCode,
                cancellationToken))
        {
            return Result<CompetencyDto>.Fail(
                "A competency with this code already exists.",
                CompetencyErrors.DuplicateCompetencyCode);
        }

        var entity = new Competency
        {
            Code = normalizedCode,
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CompetencyCategoryId = request.CompetencyCategoryId
        };

        _db.Competencies.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyDto>> UpdateAsync(
        Guid id,
        UpdateCompetencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CompetencyDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.Competencies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CompetencyDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.CompetencyNotFound);
        }

        var categoryExists = await _db.CompetencyCategories.AsNoTracking()
            .AnyAsync(x => x.Id == request.CompetencyCategoryId, cancellationToken);
        if (!categoryExists)
        {
            return Result<CompetencyDto>.Fail(
                "The competency category was not found.",
                CompetencyErrors.CompetencyCategoryMissing);
        }

        var normalizedCode = request.Code.Trim();
        if (await _db.Competencies.AsNoTracking().AnyAsync(
                x => x.Code == normalizedCode && x.Id != id,
                cancellationToken))
        {
            return Result<CompetencyDto>.Fail(
                "A competency with this code already exists.",
                CompetencyErrors.DuplicateCompetencyCode);
        }

        entity.Code = normalizedCode;
        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.CompetencyCategoryId = request.CompetencyCategoryId;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CompetencyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CompetencyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Competencies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<CompetencyDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.CompetencyNotFound);
        }

        return Result<CompetencyDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<CompetencyDto>>> GetPagedAsync(
        CompetencyFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<CompetencyDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<Competency> query = _db.Competencies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.Code.Contains(term) ||
                x.NameEn.Contains(term) ||
                x.NameAr.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompetencyDto
            {
                Id = x.Id,
                Code = x.Code,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Description = x.Description,
                CompetencyCategoryId = x.CompetencyCategoryId
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CompetencyDto>>.Ok(new PagedResult<CompetencyDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static CompetencyDto MapToDto(Competency entity) =>
        new()
        {
            Id = entity.Id,
            Code = entity.Code,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Description = entity.Description,
            CompetencyCategoryId = entity.CompetencyCategoryId
        };
}
