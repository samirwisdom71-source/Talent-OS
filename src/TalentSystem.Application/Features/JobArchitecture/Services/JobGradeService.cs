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

public sealed class JobGradeService : IJobGradeService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateJobGradeRequest> _createValidator;
    private readonly IValidator<UpdateJobGradeRequest> _updateValidator;
    private readonly IValidator<JobGradeFilterRequest> _filterValidator;

    public JobGradeService(
        TalentDbContext db,
        IValidator<CreateJobGradeRequest> createValidator,
        IValidator<UpdateJobGradeRequest> updateValidator,
        IValidator<JobGradeFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<JobGradeDto>> CreateAsync(CreateJobGradeRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<JobGradeDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var name = request.Name.Trim();
        var exists = await _db.JobGrades.AsNoTracking().AnyAsync(x => x.Name == name, cancellationToken);
        if (exists)
        {
            return Result<JobGradeDto>.Fail("A job grade with this name already exists.");
        }

        var entity = new JobGrade { Name = name, Level = request.Level };
        _db.JobGrades.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<JobGradeDto>.Ok(Map(entity));
    }

    public async Task<Result<JobGradeDto>> UpdateAsync(Guid id, UpdateJobGradeRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<JobGradeDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var entity = await _db.JobGrades.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return Result<JobGradeDto>.Fail("The job grade was not found.");
        }

        var name = request.Name.Trim();
        var exists = await _db.JobGrades.AsNoTracking().AnyAsync(x => x.Name == name && x.Id != id, cancellationToken);
        if (exists)
        {
            return Result<JobGradeDto>.Fail("A job grade with this name already exists.");
        }

        entity.Name = name;
        entity.Level = request.Level;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<JobGradeDto>.Ok(Map(entity));
    }

    public async Task<Result<JobGradeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _db.JobGrades.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new JobGradeDto
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? Result<JobGradeDto>.Fail("The job grade was not found.")
            : Result<JobGradeDto>.Ok(item);
    }

    public async Task<Result<PagedResult<JobGradeDto>>> GetPagedAsync(JobGradeFilterRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<JobGradeDto>>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize) pageSize = PaginationConstants.MaxPageSize;

        IQueryable<JobGrade> query = _db.JobGrades.AsNoTracking();

        if (request.Level.HasValue)
        {
            query = query.Where(x => x.Level == request.Level.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Level).ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new JobGradeDto { Id = x.Id, Name = x.Name, Level = x.Level })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<JobGradeDto>>.Ok(new PagedResult<JobGradeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    private static JobGradeDto Map(JobGrade x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Level = x.Level
    };
}
