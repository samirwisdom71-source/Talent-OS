using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Application.Features.Performance.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Performance.Services;

public sealed class PerformanceCycleService : IPerformanceCycleService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreatePerformanceCycleRequest> _createValidator;
    private readonly IValidator<UpdatePerformanceCycleRequest> _updateValidator;
    private readonly IValidator<PerformanceCycleFilterRequest> _filterValidator;

    public PerformanceCycleService(
        TalentDbContext db,
        IValidator<CreatePerformanceCycleRequest> createValidator,
        IValidator<UpdatePerformanceCycleRequest> updateValidator,
        IValidator<PerformanceCycleFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PerformanceCycleDto>> CreateAsync(
        CreatePerformanceCycleRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceCycleDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = new PerformanceCycle
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = PerformanceCycleStatus.Draft,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _db.PerformanceCycles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceCycleDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceCycleDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceCycleRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PerformanceCycleDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<PerformanceCycleDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (entity.Status is PerformanceCycleStatus.Closed or PerformanceCycleStatus.Archived)
        {
            return Result<PerformanceCycleDto>.Fail(
                "This performance cycle is read-only.",
                PerformanceErrors.CycleReadOnly);
        }

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceCycleDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PerformanceCycleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PerformanceCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<PerformanceCycleDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        return Result<PerformanceCycleDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<PerformanceCycleDto>>> GetPagedAsync(
        PerformanceCycleFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<PerformanceCycleDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<PerformanceCycle> query = _db.PerformanceCycles.AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

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
            .OrderByDescending(x => x.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PerformanceCycleDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<PerformanceCycleDto>>.Ok(new PagedResult<PerformanceCycleDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetLookupAsync(
        PerformanceCycleLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        var take = request.Take <= 0 ? PaginationConstants.MaxPageSize : request.Take;
        if (take > PaginationConstants.MaxPageSize)
        {
            take = PaginationConstants.MaxPageSize;
        }

        var preferEn = string.Equals(request.Lang, "en", StringComparison.OrdinalIgnoreCase);

        IQueryable<PerformanceCycle> query = _db.PerformanceCycles.AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.NameEn.Contains(term) ||
                x.NameAr.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)));
        }

        var rows = await query
            .OrderByDescending(x => x.StartDate)
            .Take(take)
            .Select(x => new { x.Id, x.NameAr, x.NameEn })
            .ToListAsync(cancellationToken);

        var list = rows
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = preferEn
                    ? PickDisplayName(x.NameEn, x.NameAr, x.Id)
                    : PickDisplayName(x.NameAr, x.NameEn, x.Id),
                Email = null
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(list);
    }

    public async Task<Result<PerformanceCycleDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        Result<PerformanceCycleDto> result = Result<PerformanceCycleDto>.Fail("Unable to activate performance cycle.");

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var entity = await _db.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
            {
                result = Result<PerformanceCycleDto>.Fail(
                    "The performance cycle was not found.",
                    PerformanceErrors.CycleNotFound);
                return;
            }

            if (entity.Status != PerformanceCycleStatus.Draft)
            {
                result = Result<PerformanceCycleDto>.Fail(
                    "Only draft performance cycles can be activated.",
                    PerformanceErrors.CycleMustBeDraftToActivate);
                return;
            }

            await _db.PerformanceCycles
                .Where(x => x.Status == PerformanceCycleStatus.Active && x.Id != id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.Status, PerformanceCycleStatus.Closed),
                    cancellationToken);

            entity.Status = PerformanceCycleStatus.Active;
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result = Result<PerformanceCycleDto>.Ok(MapToDto(entity));
        });

        return result;
    }

    public async Task<Result<PerformanceCycleDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<PerformanceCycleDto>.Fail(
                "The performance cycle was not found.",
                PerformanceErrors.CycleNotFound);
        }

        if (entity.Status != PerformanceCycleStatus.Active)
        {
            return Result<PerformanceCycleDto>.Fail(
                "Only active performance cycles can be closed.",
                PerformanceErrors.CycleMustBeActiveToClose);
        }

        entity.Status = PerformanceCycleStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PerformanceCycleDto>.Ok(MapToDto(entity));
    }

    private static PerformanceCycleDto MapToDto(PerformanceCycle entity) =>
        new()
        {
            Id = entity.Id,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            Description = entity.Description
        };

    private static string PickDisplayName(string? primary, string? secondary, Guid id)
    {
        if (!string.IsNullOrWhiteSpace(primary))
        {
            return primary.Trim();
        }

        if (!string.IsNullOrWhiteSpace(secondary))
        {
            return secondary.Trim();
        }

        return id.ToString();
    }
}
