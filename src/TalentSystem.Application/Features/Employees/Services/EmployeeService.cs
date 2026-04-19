using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Employees.DTOs;
using TalentSystem.Application.Features.Employees.Interfaces;
using TalentSystem.Domain.Employees;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Employees.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateEmployeeRequest> _createValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateValidator;
    private readonly IValidator<EmployeeFilterRequest> _filterValidator;

    public EmployeeService(
        TalentDbContext db,
        IValidator<CreateEmployeeRequest> createValidator,
        IValidator<UpdateEmployeeRequest> updateValidator,
        IValidator<EmployeeFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<EmployeeDto>> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<EmployeeDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var organizationUnitExists = await _db.OrganizationUnits.AsNoTracking()
            .AnyAsync(x => x.Id == request.OrganizationUnitId, cancellationToken);
        if (!organizationUnitExists)
        {
            return Result<EmployeeDto>.Fail(
                "The organization unit was not found.",
                EmployeeErrors.OrganizationUnitNotFound);
        }

        var position = await _db.Positions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PositionId, cancellationToken);
        if (position is null)
        {
            return Result<EmployeeDto>.Fail(
                "The position was not found.",
                EmployeeErrors.PositionNotFound);
        }

        if (position.OrganizationUnitId != request.OrganizationUnitId)
        {
            return Result<EmployeeDto>.Fail(
                "The position does not belong to the specified organization unit.",
                EmployeeErrors.PositionOrganizationMismatch);
        }

        var normalizedNumber = request.EmployeeNumber.Trim();
        var normalizedEmail = request.Email.Trim();

        if (await _db.Employees.AsNoTracking().AnyAsync(
                e => e.EmployeeNumber == normalizedNumber,
                cancellationToken))
        {
            return Result<EmployeeDto>.Fail(
                "An employee with this employee number already exists.",
                EmployeeErrors.DuplicateEmployeeNumber);
        }

        if (await _db.Employees.AsNoTracking().AnyAsync(
                e => e.Email == normalizedEmail,
                cancellationToken))
        {
            return Result<EmployeeDto>.Fail(
                "An employee with this email address already exists.",
                EmployeeErrors.DuplicateEmail);
        }

        var employee = new Employee
        {
            EmployeeNumber = normalizedNumber,
            FullNameAr = request.FullNameAr.Trim(),
            FullNameEn = request.FullNameEn.Trim(),
            Email = normalizedEmail,
            OrganizationUnitId = request.OrganizationUnitId,
            PositionId = request.PositionId
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Ok(MapToDto(employee));
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<EmployeeDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            return Result<EmployeeDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        var organizationUnitExists = await _db.OrganizationUnits.AsNoTracking()
            .AnyAsync(x => x.Id == request.OrganizationUnitId, cancellationToken);
        if (!organizationUnitExists)
        {
            return Result<EmployeeDto>.Fail(
                "The organization unit was not found.",
                EmployeeErrors.OrganizationUnitNotFound);
        }

        var position = await _db.Positions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PositionId, cancellationToken);
        if (position is null)
        {
            return Result<EmployeeDto>.Fail(
                "The position was not found.",
                EmployeeErrors.PositionNotFound);
        }

        if (position.OrganizationUnitId != request.OrganizationUnitId)
        {
            return Result<EmployeeDto>.Fail(
                "The position does not belong to the specified organization unit.",
                EmployeeErrors.PositionOrganizationMismatch);
        }

        var normalizedNumber = request.EmployeeNumber.Trim();
        var normalizedEmail = request.Email.Trim();

        if (await _db.Employees.AsNoTracking().AnyAsync(
                e => e.EmployeeNumber == normalizedNumber && e.Id != id,
                cancellationToken))
        {
            return Result<EmployeeDto>.Fail(
                "An employee with this employee number already exists.",
                EmployeeErrors.DuplicateEmployeeNumber);
        }

        if (await _db.Employees.AsNoTracking().AnyAsync(
                e => e.Email == normalizedEmail && e.Id != id,
                cancellationToken))
        {
            return Result<EmployeeDto>.Fail(
                "An employee with this email address already exists.",
                EmployeeErrors.DuplicateEmail);
        }

        employee.EmployeeNumber = normalizedNumber;
        employee.FullNameAr = request.FullNameAr.Trim();
        employee.FullNameEn = request.FullNameEn.Trim();
        employee.Email = normalizedEmail;
        employee.OrganizationUnitId = request.OrganizationUnitId;
        employee.PositionId = request.PositionId;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Ok(MapToDto(employee));
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
        {
            return Result<EmployeeDto>.Fail(
                "The employee was not found.",
                EmployeeErrors.NotFound);
        }

        return Result<EmployeeDto>.Ok(MapToDto(employee));
    }

    public async Task<Result<PagedResult<EmployeeListItemDto>>> GetPagedAsync(
        EmployeeFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<PagedResult<EmployeeListItemDto>>.Fail(
                validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        var query = _db.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(e =>
                e.EmployeeNumber.Contains(term) ||
                e.FullNameEn.Contains(term) ||
                e.FullNameAr.Contains(term) ||
                e.Email.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.FullNameEn)
            .ThenBy(e => e.EmployeeNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeListItemDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FullNameAr = e.FullNameAr,
                FullNameEn = e.FullNameEn,
                Email = e.Email,
                OrganizationUnitId = e.OrganizationUnitId,
                PositionId = e.PositionId
            })
            .ToListAsync(cancellationToken);

        var paged = new PagedResult<EmployeeListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<EmployeeListItemDto>>.Ok(paged);
    }

    private static EmployeeDto MapToDto(Employee employee) =>
        new()
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FullNameAr = employee.FullNameAr,
            FullNameEn = employee.FullNameEn,
            Email = employee.Email,
            OrganizationUnitId = employee.OrganizationUnitId,
            PositionId = employee.PositionId
        };
}
