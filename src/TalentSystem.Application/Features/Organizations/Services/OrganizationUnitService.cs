using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Organizations.DTOs;
using TalentSystem.Application.Features.Organizations.Interfaces;
using TalentSystem.Domain.Organizations;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Organizations.Services;

public sealed class OrganizationUnitService : IOrganizationUnitService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateOrganizationUnitRequest> _createValidator;
    private readonly IValidator<UpdateOrganizationUnitRequest> _updateValidator;
    private readonly IValidator<OrganizationUnitFilterRequest> _filterValidator;

    public OrganizationUnitService(
        TalentDbContext db,
        IValidator<CreateOrganizationUnitRequest> createValidator,
        IValidator<UpdateOrganizationUnitRequest> updateValidator,
        IValidator<OrganizationUnitFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<OrganizationUnitDto>> CreateAsync(
        CreateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OrganizationUnitDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _db.OrganizationUnits.AsNoTracking()
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);
            if (!parentExists)
            {
                return Result<OrganizationUnitDto>.Fail("The selected parent organization unit was not found.");
            }
        }

        var entity = new OrganizationUnit
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            ParentId = request.ParentId
        };

        _db.OrganizationUnits.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result<OrganizationUnitDto>> UpdateAsync(
        Guid id,
        UpdateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<OrganizationUnitDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.OrganizationUnits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<OrganizationUnitDto>.Fail("The organization unit was not found.");
        }

        if (request.ParentId == id)
        {
            return Result<OrganizationUnitDto>.Fail("An organization unit cannot be its own parent.");
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _db.OrganizationUnits.AsNoTracking()
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);
            if (!parentExists)
            {
                return Result<OrganizationUnitDto>.Fail("The selected parent organization unit was not found.");
            }

            var createsCycle = await IsDescendantAsync(request.ParentId.Value, id, cancellationToken);
            if (createsCycle)
            {
                return Result<OrganizationUnitDto>.Fail("The selected parent creates a cyclic hierarchy.");
            }
        }

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.ParentId = request.ParentId;

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<OrganizationUnitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _db.OrganizationUnits.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new OrganizationUnitDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                ParentId = x.ParentId,
                ParentNameAr = x.Parent != null ? x.Parent.NameAr : null,
                ParentNameEn = x.Parent != null ? x.Parent.NameEn : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? Result<OrganizationUnitDto>.Fail("The organization unit was not found.")
            : Result<OrganizationUnitDto>.Ok(item);
    }

    public async Task<Result<PagedResult<OrganizationUnitDto>>> GetPagedAsync(
        OrganizationUnitFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<OrganizationUnitDto>>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<OrganizationUnit> query = _db.OrganizationUnits.AsNoTracking();

        if (request.ParentId.HasValue)
        {
            query = query.Where(x => x.ParentId == request.ParentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.NameAr.Contains(term) || x.NameEn.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.NameAr)
            .ThenBy(x => x.NameEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrganizationUnitDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                ParentId = x.ParentId,
                ParentNameAr = x.Parent != null ? x.Parent.NameAr : null,
                ParentNameEn = x.Parent != null ? x.Parent.NameEn : null
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OrganizationUnitDto>>.Ok(new PagedResult<OrganizationUnitDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private async Task<bool> IsDescendantAsync(Guid potentialParentId, Guid currentId, CancellationToken cancellationToken)
    {
        var nextParentId = potentialParentId;
        while (nextParentId != Guid.Empty)
        {
            if (nextParentId == currentId)
            {
                return true;
            }

            var node = await _db.OrganizationUnits.AsNoTracking()
                .Where(x => x.Id == nextParentId)
                .Select(x => new { x.ParentId })
                .FirstOrDefaultAsync(cancellationToken);

            if (node is null || !node.ParentId.HasValue)
            {
                return false;
            }

            nextParentId = node.ParentId.Value;
        }

        return false;
    }
}
