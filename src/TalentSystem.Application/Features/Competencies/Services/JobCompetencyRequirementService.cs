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

public sealed class JobCompetencyRequirementService : IJobCompetencyRequirementService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateJobCompetencyRequirementRequest> _createValidator;
    private readonly IValidator<UpdateJobCompetencyRequirementRequest> _updateValidator;
    private readonly IValidator<JobCompetencyRequirementFilterRequest> _filterValidator;

    public JobCompetencyRequirementService(
        TalentDbContext db,
        IValidator<CreateJobCompetencyRequirementRequest> createValidator,
        IValidator<UpdateJobCompetencyRequirementRequest> updateValidator,
        IValidator<JobCompetencyRequirementFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<JobCompetencyRequirementDto>> CreateAsync(
        CreateJobCompetencyRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<JobCompetencyRequirementDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var positionExists = await _db.Positions.AsNoTracking()
            .AnyAsync(x => x.Id == request.PositionId, cancellationToken);
        if (!positionExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The position was not found.",
                CompetencyErrors.JobRequirementPositionNotFound);
        }

        var competencyExists = await _db.Competencies.AsNoTracking()
            .AnyAsync(x => x.Id == request.CompetencyId, cancellationToken);
        if (!competencyExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.JobRequirementCompetencyNotFound);
        }

        var levelExists = await _db.CompetencyLevels.AsNoTracking()
            .AnyAsync(x => x.Id == request.RequiredLevelId, cancellationToken);
        if (!levelExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The required competency level was not found.",
                CompetencyErrors.JobRequirementRequiredLevelNotFound);
        }

        if (await _db.JobCompetencyRequirements.AsNoTracking().AnyAsync(
                x => x.PositionId == request.PositionId && x.CompetencyId == request.CompetencyId,
                cancellationToken))
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "This position already has a requirement for the selected competency.",
                CompetencyErrors.DuplicateJobRequirement);
        }

        var entity = new JobCompetencyRequirement
        {
            PositionId = request.PositionId,
            CompetencyId = request.CompetencyId,
            RequiredLevelId = request.RequiredLevelId
        };

        _db.JobCompetencyRequirements.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<JobCompetencyRequirementDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<JobCompetencyRequirementDto>> UpdateAsync(
        Guid id,
        UpdateJobCompetencyRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<JobCompetencyRequirementDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.JobCompetencyRequirements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The job competency requirement was not found.",
                CompetencyErrors.JobRequirementNotFound);
        }

        var positionExists = await _db.Positions.AsNoTracking()
            .AnyAsync(x => x.Id == request.PositionId, cancellationToken);
        if (!positionExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The position was not found.",
                CompetencyErrors.JobRequirementPositionNotFound);
        }

        var competencyExists = await _db.Competencies.AsNoTracking()
            .AnyAsync(x => x.Id == request.CompetencyId, cancellationToken);
        if (!competencyExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The competency was not found.",
                CompetencyErrors.JobRequirementCompetencyNotFound);
        }

        var levelExists = await _db.CompetencyLevels.AsNoTracking()
            .AnyAsync(x => x.Id == request.RequiredLevelId, cancellationToken);
        if (!levelExists)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The required competency level was not found.",
                CompetencyErrors.JobRequirementRequiredLevelNotFound);
        }

        if (await _db.JobCompetencyRequirements.AsNoTracking().AnyAsync(
                x => x.PositionId == request.PositionId &&
                     x.CompetencyId == request.CompetencyId &&
                     x.Id != id,
                cancellationToken))
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "This position already has a requirement for the selected competency.",
                CompetencyErrors.DuplicateJobRequirement);
        }

        entity.PositionId = request.PositionId;
        entity.CompetencyId = request.CompetencyId;
        entity.RequiredLevelId = request.RequiredLevelId;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<JobCompetencyRequirementDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<JobCompetencyRequirementDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.JobCompetencyRequirements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result<JobCompetencyRequirementDto>.Fail(
                "The job competency requirement was not found.",
                CompetencyErrors.JobRequirementNotFound);
        }

        return Result<JobCompetencyRequirementDto>.Ok(MapToDto(entity));
    }

    public async Task<Result<PagedResult<JobCompetencyRequirementDto>>> GetPagedAsync(
        JobCompetencyRequirementFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<JobCompetencyRequirementDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        IQueryable<JobCompetencyRequirement> query = _db.JobCompetencyRequirements.AsNoTracking();

        if (request.PositionId is { } positionId && positionId != Guid.Empty)
        {
            query = query.Where(x => x.PositionId == positionId);
        }

        if (request.CompetencyId is { } competencyId && competencyId != Guid.Empty)
        {
            query = query.Where(x => x.CompetencyId == competencyId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.PositionId)
            .ThenBy(x => x.CompetencyId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new JobCompetencyRequirementDto
            {
                Id = x.Id,
                PositionId = x.PositionId,
                CompetencyId = x.CompetencyId,
                RequiredLevelId = x.RequiredLevelId
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<JobCompetencyRequirementDto>>.Ok(new PagedResult<JobCompetencyRequirementDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static JobCompetencyRequirementDto MapToDto(JobCompetencyRequirement entity) =>
        new()
        {
            Id = entity.Id,
            PositionId = entity.PositionId,
            CompetencyId = entity.CompetencyId,
            RequiredLevelId = entity.RequiredLevelId
        };
}
