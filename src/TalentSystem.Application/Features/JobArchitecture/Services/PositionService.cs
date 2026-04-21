using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Application.Features.JobArchitecture.Interfaces;
using TalentSystem.Domain.JobArchitecture;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.JobArchitecture.Services;

public sealed class PositionService : IPositionService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreatePositionRequest> _createValidator;
    private readonly IValidator<UpdatePositionRequest> _updateValidator;
    private readonly IValidator<PositionFilterRequest> _filterValidator;

    public PositionService(
        TalentDbContext db,
        IValidator<CreatePositionRequest> createValidator,
        IValidator<UpdatePositionRequest> updateValidator,
        IValidator<PositionFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PositionDto>> CreateAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PositionDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var referencesValidation = await ValidateReferences(request.OrganizationUnitId, request.JobGradeId, cancellationToken);
        if (!referencesValidation.IsSuccess)
        {
            return Result<PositionDto>.Fail(referencesValidation.Errors);
        }

        var entity = new Position
        {
            TitleAr = request.TitleAr.Trim(),
            TitleEn = request.TitleEn.Trim(),
            OrganizationUnitId = request.OrganizationUnitId,
            JobGradeId = request.JobGradeId
        };

        _db.Positions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result<PositionDto>> UpdateAsync(
        Guid id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PositionDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.Positions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<PositionDto>.Fail("The position was not found.");
        }

        var referencesValidation = await ValidateReferences(request.OrganizationUnitId, request.JobGradeId, cancellationToken);
        if (!referencesValidation.IsSuccess)
        {
            return Result<PositionDto>.Fail(referencesValidation.Errors);
        }

        entity.TitleAr = request.TitleAr.Trim();
        entity.TitleEn = request.TitleEn.Trim();
        entity.OrganizationUnitId = request.OrganizationUnitId;
        entity.JobGradeId = request.JobGradeId;

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<PositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _db.Positions.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PositionDto
            {
                Id = x.Id,
                TitleAr = x.TitleAr,
                TitleEn = x.TitleEn,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitNameAr = x.OrganizationUnit.NameAr,
                OrganizationUnitNameEn = x.OrganizationUnit.NameEn,
                JobGradeId = x.JobGradeId,
                JobGradeName = x.JobGrade.Name,
                JobGradeLevel = x.JobGrade.Level
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? Result<PositionDto>.Fail("The position was not found.")
            : Result<PositionDto>.Ok(item);
    }

    public async Task<Result<PagedResult<PositionDto>>> GetPagedAsync(
        PositionFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<PositionDto>>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<Position> query = _db.Positions.AsNoTracking();

        if (request.OrganizationUnitId.HasValue)
        {
            var organizationUnitIds = await GetRelatedOrganizationUnitIdsAsync(
                request.OrganizationUnitId.Value,
                cancellationToken);
            query = query.Where(x => organizationUnitIds.Contains(x.OrganizationUnitId));
        }

        if (request.JobGradeId.HasValue)
        {
            query = query.Where(x => x.JobGradeId == request.JobGradeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.TitleAr.Contains(term) || x.TitleEn.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.TitleAr)
            .ThenBy(x => x.TitleEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PositionDto
            {
                Id = x.Id,
                TitleAr = x.TitleAr,
                TitleEn = x.TitleEn,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitNameAr = x.OrganizationUnit.NameAr,
                OrganizationUnitNameEn = x.OrganizationUnit.NameEn,
                JobGradeId = x.JobGradeId,
                JobGradeName = x.JobGrade.Name,
                JobGradeLevel = x.JobGrade.Level
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<PositionDto>>.Ok(new PagedResult<PositionDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private async Task<Result<bool>> ValidateReferences(
        Guid organizationUnitId,
        Guid jobGradeId,
        CancellationToken cancellationToken)
    {
        var orgExists = await _db.OrganizationUnits.AsNoTracking()
            .AnyAsync(x => x.Id == organizationUnitId, cancellationToken);
        if (!orgExists)
        {
            return Result<bool>.Fail("The selected organization unit was not found.");
        }

        var gradeExists = await _db.JobGrades.AsNoTracking()
            .AnyAsync(x => x.Id == jobGradeId, cancellationToken);
        if (!gradeExists)
        {
            return Result<bool>.Fail("The selected job grade was not found.");
        }

        return Result<bool>.Ok(true);
    }

    private async Task<IReadOnlyCollection<Guid>> GetRelatedOrganizationUnitIdsAsync(
        Guid rootId,
        CancellationToken cancellationToken)
    {
        var allUnits = await _db.OrganizationUnits.AsNoTracking()
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync(cancellationToken);

        var parentByChild = allUnits.ToDictionary(x => x.Id, x => x.ParentId);
        var childrenByParent = allUnits
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToArray());

        var collected = new HashSet<Guid> { rootId };
        var downQueue = new Queue<Guid>();
        downQueue.Enqueue(rootId);

        while (downQueue.Count > 0)
        {
            var current = downQueue.Dequeue();
            if (!childrenByParent.TryGetValue(current, out var children))
            {
                continue;
            }

            foreach (var childId in children)
            {
                if (collected.Add(childId))
                {
                    downQueue.Enqueue(childId);
                }
            }
        }

        var currentParent = rootId;
        while (parentByChild.TryGetValue(currentParent, out var parentId) && parentId.HasValue)
        {
            if (!collected.Add(parentId.Value))
            {
                break;
            }

            currentParent = parentId.Value;
        }

        return collected;
    }
}
