using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Application.Features.Succession.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Succession;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Succession.Services;

public sealed class CriticalPositionService : ICriticalPositionService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateCriticalPositionRequest> _createValidator;
    private readonly IValidator<UpdateCriticalPositionRequest> _updateValidator;
    private readonly IValidator<CriticalPositionFilterRequest> _filterValidator;

    public CriticalPositionService(
        TalentDbContext db,
        IValidator<CreateCriticalPositionRequest> createValidator,
        IValidator<UpdateCriticalPositionRequest> updateValidator,
        IValidator<CriticalPositionFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<CriticalPositionDto>> CreateAsync(
        CreateCriticalPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CriticalPositionDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        if (!await _db.Positions.AsNoTracking().AnyAsync(x => x.Id == request.PositionId, cancellationToken))
        {
            return Result<CriticalPositionDto>.Fail(
                "The position was not found.",
                EmployeeErrors.PositionNotFound);
        }

        if (await _db.CriticalPositions.AsNoTracking().AnyAsync(
                x => x.PositionId == request.PositionId && x.RecordStatus == RecordStatus.Active,
                cancellationToken))
        {
            return Result<CriticalPositionDto>.Fail(
                "This position is already designated as an active critical position.",
                SuccessionErrors.CriticalPositionDuplicate);
        }

        var entity = new CriticalPosition
        {
            PositionId = request.PositionId,
            CriticalityLevel = request.CriticalityLevel,
            RiskLevel = request.RiskLevel,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RecordStatus = RecordStatus.Active
        };

        _db.CriticalPositions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CriticalPositionDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CriticalPositionDto>> UpdateAsync(
        Guid id,
        UpdateCriticalPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<CriticalPositionDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.CriticalPositions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CriticalPositionDto>.Fail(
                "The critical position was not found.",
                SuccessionErrors.CriticalPositionNotFound);
        }

        if (entity.RecordStatus != RecordStatus.Active)
        {
            return Result<CriticalPositionDto>.Fail(
                "The critical position is archived and cannot be updated.",
                SuccessionErrors.CriticalPositionReadOnly);
        }

        entity.CriticalityLevel = request.CriticalityLevel;
        entity.RiskLevel = request.RiskLevel;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CriticalPositionDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<CriticalPositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CriticalPositions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<CriticalPositionDto>.Fail(
                "The critical position was not found.",
                SuccessionErrors.CriticalPositionNotFound);
        }

        return Result<CriticalPositionDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<CriticalPositionDto>>> GetPagedAsync(
        CriticalPositionFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<CriticalPositionDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<CriticalPosition> query = _db.CriticalPositions.AsNoTracking();

        if (request.PositionId is { } positionId && positionId != Guid.Empty)
        {
            query = query.Where(x => x.PositionId == positionId);
        }

        if (request.ActiveOnly == true)
        {
            query = query.Where(x => x.RecordStatus == RecordStatus.Active);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CriticalPositionDto
            {
                Id = x.Id,
                PositionId = x.PositionId,
                CriticalityLevel = x.CriticalityLevel,
                RiskLevel = x.RiskLevel,
                Notes = x.Notes,
                RecordStatus = x.RecordStatus
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CriticalPositionDto>>.Ok(new PagedResult<CriticalPositionDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<CriticalPositionDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CriticalPositions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<CriticalPositionDto>.Fail(
                "The critical position was not found.",
                SuccessionErrors.CriticalPositionNotFound);
        }

        if (entity.RecordStatus != RecordStatus.Active)
        {
            return Result<CriticalPositionDto>.Ok(MapToDto(entity));
        }

        entity.RecordStatus = RecordStatus.Archived;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CriticalPositionDto>.Ok(MapToDto(entity));
    }

    private static CriticalPositionDto MapToDto(CriticalPosition entity) =>
        new()
        {
            Id = entity.Id,
            PositionId = entity.PositionId,
            CriticalityLevel = entity.CriticalityLevel,
            RiskLevel = entity.RiskLevel,
            Notes = entity.Notes,
            RecordStatus = entity.RecordStatus
        };
}
