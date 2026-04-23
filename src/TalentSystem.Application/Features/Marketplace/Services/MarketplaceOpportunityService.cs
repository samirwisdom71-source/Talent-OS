using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Marketplace;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Marketplace.Services;

public sealed class MarketplaceOpportunityService : IMarketplaceOpportunityService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateMarketplaceOpportunityRequest> _createValidator;
    private readonly IValidator<UpdateMarketplaceOpportunityRequest> _updateValidator;
    private readonly IValidator<MarketplaceOpportunityFilterRequest> _filterValidator;

    public MarketplaceOpportunityService(
        TalentDbContext db,
        IValidator<CreateMarketplaceOpportunityRequest> createValidator,
        IValidator<UpdateMarketplaceOpportunityRequest> updateValidator,
        IValidator<MarketplaceOpportunityFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<MarketplaceOpportunityDto>> CreateAsync(
        CreateMarketplaceOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<MarketplaceOpportunityDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.OrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == request.OrganizationUnitId, cancellationToken))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The organization unit was not found.",
                EmployeeErrors.OrganizationUnitNotFound);
        }

        if (request.PositionId is { } positionId &&
            !await _db.Positions.AsNoTracking().AnyAsync(x => x.Id == positionId, cancellationToken))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The position was not found.",
                EmployeeErrors.PositionNotFound);
        }

        var entity = new MarketplaceOpportunity
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            OpportunityType = request.OpportunityType,
            OrganizationUnitId = request.OrganizationUnitId,
            PositionId = request.PositionId,
            RequiredCompetencySummary = string.IsNullOrWhiteSpace(request.RequiredCompetencySummary)
                ? null
                : request.RequiredCompetencySummary.Trim(),
            Status = MarketplaceOpportunityStatus.Draft,
            OpenDate = request.OpenDate,
            CloseDate = request.CloseDate,
            MaxApplicants = request.MaxApplicants,
            IsConfidential = request.IsConfidential,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        _db.MarketplaceOpportunities.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<MarketplaceOpportunityDto>> UpdateAsync(
        Guid id,
        UpdateMarketplaceOpportunityRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<MarketplaceOpportunityDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.MarketplaceOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        if (!OpportunityAllowsMetadataEdit(entity.Status))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity cannot be updated in its current status.",
                MarketplaceErrors.OpportunityReadOnly);
        }

        if (!await _db.OrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == request.OrganizationUnitId, cancellationToken))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The organization unit was not found.",
                EmployeeErrors.OrganizationUnitNotFound);
        }

        if (request.PositionId is { } positionId &&
            !await _db.Positions.AsNoTracking().AnyAsync(x => x.Id == positionId, cancellationToken))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The position was not found.",
                EmployeeErrors.PositionNotFound);
        }

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.OpportunityType = request.OpportunityType;
        entity.OrganizationUnitId = request.OrganizationUnitId;
        entity.PositionId = request.PositionId;
        entity.RequiredCompetencySummary = string.IsNullOrWhiteSpace(request.RequiredCompetencySummary)
            ? null
            : request.RequiredCompetencySummary.Trim();
        entity.OpenDate = request.OpenDate;
        entity.CloseDate = request.CloseDate;
        entity.MaxApplicants = request.MaxApplicants;
        entity.IsConfidential = request.IsConfidential;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<MarketplaceOpportunityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await (
            from m in _db.MarketplaceOpportunities.AsNoTracking()
            join ou in _db.OrganizationUnits.AsNoTracking() on m.OrganizationUnitId equals ou.Id
            join p in _db.Positions.AsNoTracking() on m.PositionId equals p.Id into posJoin
            from p in posJoin.DefaultIfEmpty()
            where m.Id == id
            select new MarketplaceOpportunityDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                OpportunityType = m.OpportunityType,
                OrganizationUnitId = m.OrganizationUnitId,
                OrganizationUnitName = string.IsNullOrWhiteSpace(ou.NameAr) ? ou.NameEn : ou.NameAr,
                PositionId = m.PositionId,
                PositionTitle = p == null
                    ? null
                    : (string.IsNullOrWhiteSpace(p.TitleAr) ? p.TitleEn : p.TitleAr),
                RequiredCompetencySummary = m.RequiredCompetencySummary,
                Status = m.Status,
                OpenDate = m.OpenDate,
                CloseDate = m.CloseDate,
                MaxApplicants = m.MaxApplicants,
                IsConfidential = m.IsConfidential,
                Notes = m.Notes
            }).FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        return Result<MarketplaceOpportunityDto>.Ok(dto);
    }

    public async Task<Result<PagedResult<MarketplaceOpportunityDto>>> GetPagedAsync(
        MarketplaceOpportunityFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<MarketplaceOpportunityDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<MarketplaceOpportunity> query = _db.MarketplaceOpportunities.AsNoTracking();

        if (request.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (request.OpportunityType is { } type)
        {
            query = query.Where(x => x.OpportunityType == type);
        }

        if (request.OrganizationUnitId is { } ouId && ouId != Guid.Empty)
        {
            query = query.Where(x => x.OrganizationUnitId == ouId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var baseQuery =
            from m in query
            join ou in _db.OrganizationUnits.AsNoTracking() on m.OrganizationUnitId equals ou.Id
            join p in _db.Positions.AsNoTracking() on m.PositionId equals p.Id into posJoin
            from p in posJoin.DefaultIfEmpty()
            select new { m, ou, p };

        var items = await baseQuery
            .OrderByDescending(x => x.m.OpenDate)
            .ThenBy(x => x.m.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MarketplaceOpportunityDto
            {
                Id = x.m.Id,
                Title = x.m.Title,
                Description = x.m.Description,
                OpportunityType = x.m.OpportunityType,
                OrganizationUnitId = x.m.OrganizationUnitId,
                OrganizationUnitName = string.IsNullOrWhiteSpace(x.ou.NameAr) ? x.ou.NameEn : x.ou.NameAr,
                PositionId = x.m.PositionId,
                PositionTitle = x.p == null
                    ? null
                    : (string.IsNullOrWhiteSpace(x.p.TitleAr) ? x.p.TitleEn : x.p.TitleAr),
                RequiredCompetencySummary = x.m.RequiredCompetencySummary,
                Status = x.m.Status,
                OpenDate = x.m.OpenDate,
                CloseDate = x.m.CloseDate,
                MaxApplicants = x.m.MaxApplicants,
                IsConfidential = x.m.IsConfidential,
                Notes = x.m.Notes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<MarketplaceOpportunityDto>>.Ok(new PagedResult<MarketplaceOpportunityDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<MarketplaceOpportunityDto>> OpenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.MarketplaceOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        if (entity.Status == MarketplaceOpportunityStatus.Open)
        {
            return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
        }

        if (entity.Status != MarketplaceOpportunityStatus.Draft)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "Only a draft marketplace opportunity can be opened.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Open;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<MarketplaceOpportunityDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.MarketplaceOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        if (entity.Status == MarketplaceOpportunityStatus.Closed)
        {
            return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
        }

        if (entity.Status != MarketplaceOpportunityStatus.Open)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "Only an open marketplace opportunity can be closed.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
    }

    public async Task<Result<MarketplaceOpportunityDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.MarketplaceOpportunities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        if (entity.Status == MarketplaceOpportunityStatus.Cancelled)
        {
            return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
        }

        if (entity.Status is not (MarketplaceOpportunityStatus.Draft or MarketplaceOpportunityStatus.Open))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity cannot be cancelled from its current status.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(await ToDtoAsync(entity, cancellationToken));
    }

    private static bool OpportunityAllowsMetadataEdit(MarketplaceOpportunityStatus status) =>
        status is MarketplaceOpportunityStatus.Draft or MarketplaceOpportunityStatus.Open;

    private async Task<MarketplaceOpportunityDto> ToDtoAsync(
        MarketplaceOpportunity entity,
        CancellationToken cancellationToken)
    {
        var ou = await _db.OrganizationUnits.AsNoTracking()
            .Where(x => x.Id == entity.OrganizationUnitId)
            .Select(x => new { x.NameAr, x.NameEn })
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        string? positionTitle = null;
        if (entity.PositionId is { } pid)
        {
            positionTitle = await _db.Positions.AsNoTracking()
                .Where(x => x.Id == pid)
                .Select(x => string.IsNullOrWhiteSpace(x.TitleAr) ? x.TitleEn : x.TitleAr)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new MarketplaceOpportunityDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            OpportunityType = entity.OpportunityType,
            OrganizationUnitId = entity.OrganizationUnitId,
            OrganizationUnitName = string.IsNullOrWhiteSpace(ou.NameAr) ? ou.NameEn : ou.NameAr,
            PositionId = entity.PositionId,
            PositionTitle = positionTitle,
            RequiredCompetencySummary = entity.RequiredCompetencySummary,
            Status = entity.Status,
            OpenDate = entity.OpenDate,
            CloseDate = entity.CloseDate,
            MaxApplicants = entity.MaxApplicants,
            IsConfidential = entity.IsConfidential,
            Notes = entity.Notes
        };
    }
}
