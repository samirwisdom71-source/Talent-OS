using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Common;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Domain.Identity;
using TalentSystem.Persistence;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Constants;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Services;

public sealed class RoleService : IRoleService
{
    private readonly TalentDbContext _db;
    private readonly IValidator<CreateRoleRequest> _createValidator;
    private readonly IValidator<UpdateRoleRequest> _updateValidator;
    private readonly IValidator<RoleFilterRequest> _filterValidator;

    public RoleService(
        TalentDbContext db,
        IValidator<CreateRoleRequest> createValidator,
        IValidator<UpdateRoleRequest> updateValidator,
        IValidator<RoleFilterRequest> filterValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<RoleDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var name = request.Name.Trim();
        if (await _db.Roles.AsNoTracking()
                .AnyAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken)
                .ConfigureAwait(false))
        {
            return Result<RoleDto>.Fail("A role with this name already exists.", IdentityErrors.DuplicateRoleName);
        }

        var role = new Role
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSystemRole = request.IsSystemRole
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<RoleDto>.Ok(await MapRoleDtoAsync(role.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<RoleDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return Result<RoleDto>.Fail("The role was not found.", IdentityErrors.RoleNotFound);
        }

        var name = request.Name.Trim();
        if (role.IsSystemRole && !string.Equals(role.Name, name, StringComparison.Ordinal))
        {
            return Result<RoleDto>.Fail("System role names cannot be changed.", IdentityErrors.SystemRoleNameImmutable);
        }

        if (await _db.Roles.AsNoTracking()
                .AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.Id != id, cancellationToken)
                .ConfigureAwait(false))
        {
            return Result<RoleDto>.Fail("A role with this name already exists.", IdentityErrors.DuplicateRoleName);
        }

        role.Name = name;
        role.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<RoleDto>.Ok(await MapRoleDtoAsync(role.Id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<RoleDto>> AssignPermissionsAsync(
        Guid id,
        AssignRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return Result<RoleDto>.Fail("The role was not found.", IdentityErrors.RoleNotFound);
        }

        var permissionIds = request.PermissionIds.Distinct().ToList();
        if (permissionIds.Count > 0)
        {
            var found = await _db.Permissions.AsNoTracking().Where(p => permissionIds.Contains(p.Id)).CountAsync(cancellationToken)
                .ConfigureAwait(false);
            if (found != permissionIds.Count)
            {
                return Result<RoleDto>.Fail("One or more permissions were not found.", IdentityErrors.PermissionNotFound);
            }
        }

        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds)
        {
            _db.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permissionId });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<RoleDto>.Ok(await MapRoleDtoAsync(id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _db.Roles.AsNoTracking().AnyAsync(r => r.Id == id, cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            return Result<RoleDto>.Fail("The role was not found.", IdentityErrors.RoleNotFound);
        }

        return Result<RoleDto>.Ok(await MapRoleDtoAsync(id, cancellationToken).ConfigureAwait(false));
    }

    public async Task<Result<PagedResult<RoleListItemDto>>> GetPagedAsync(
        RoleFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _filterValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result<PagedResult<RoleListItemDto>>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var page = request.Page <= 0 ? PaginationConstants.DefaultPage : request.Page;
        var pageSize = request.PageSize <= 0 ? PaginationConstants.DefaultPageSize : request.PageSize;
        if (pageSize > PaginationConstants.MaxPageSize)
        {
            pageSize = PaginationConstants.MaxPageSize;
        }

        var query = _db.Roles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(r => r.Name.Contains(term) || (r.Description != null && r.Description.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<RoleListItemDto>>.Ok(new PagedResult<RoleListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    private async Task<RoleDto> MapRoleDtoAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.AsNoTracking()
            .Where(r => r.Id == roleId)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.IsSystemRole,
                PermissionCodes = r.RolePermissions
                    .Select(rp => rp.Permission.Code)
                    .OrderBy(c => c)
                    .ToList()
            })
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            PermissionCodes = role.PermissionCodes
        };
    }
}
