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

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
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

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<MarketplaceOpportunityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.MarketplaceOpportunities.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity was not found.",
                MarketplaceErrors.OpportunityNotFound);
        }

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
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

        var items = await query
            .OrderByDescending(x => x.OpenDate)
            .ThenBy(x => x.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MarketplaceOpportunityDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                OpportunityType = x.OpportunityType,
                OrganizationUnitId = x.OrganizationUnitId,
                PositionId = x.PositionId,
                RequiredCompetencySummary = x.RequiredCompetencySummary,
                Status = x.Status,
                OpenDate = x.OpenDate,
                CloseDate = x.CloseDate,
                MaxApplicants = x.MaxApplicants,
                IsConfidential = x.IsConfidential,
                Notes = x.Notes
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
            return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
        }

        if (entity.Status != MarketplaceOpportunityStatus.Draft)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "Only a draft marketplace opportunity can be opened.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Open;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
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
            return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
        }

        if (entity.Status != MarketplaceOpportunityStatus.Open)
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "Only an open marketplace opportunity can be closed.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
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
            return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
        }

        if (entity.Status is not (MarketplaceOpportunityStatus.Draft or MarketplaceOpportunityStatus.Open))
        {
            return Result<MarketplaceOpportunityDto>.Fail(
                "The marketplace opportunity cannot be cancelled from its current status.",
                MarketplaceErrors.InvalidOpportunityStatusTransition);
        }

        entity.Status = MarketplaceOpportunityStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<MarketplaceOpportunityDto>.Ok(MapToDto(entity));
    }

    private static bool OpportunityAllowsMetadataEdit(MarketplaceOpportunityStatus status) =>
        status is MarketplaceOpportunityStatus.Draft or MarketplaceOpportunityStatus.Open;

    private static MarketplaceOpportunityDto MapToDto(MarketplaceOpportunity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            OpportunityType = entity.OpportunityType,
            OrganizationUnitId = entity.OrganizationUnitId,
            PositionId = entity.PositionId,
            RequiredCompetencySummary = entity.RequiredCompetencySummary,
            Status = entity.Status,
            OpenDate = entity.OpenDate,
            CloseDate = entity.CloseDate,
            MaxApplicants = entity.MaxApplicants,
            IsConfidential = entity.IsConfidential,
            Notes = entity.Notes
        };
}
