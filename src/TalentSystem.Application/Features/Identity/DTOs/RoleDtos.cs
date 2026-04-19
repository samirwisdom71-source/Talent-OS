using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Identity.DTOs;

public sealed class RoleDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsSystemRole { get; init; }

    public IReadOnlyList<string> PermissionCodes { get; init; } = Array.Empty<string>();
}

public sealed class RoleListItemDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsSystemRole { get; init; }
}

public sealed class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }
}

public sealed class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class AssignRolePermissionsRequest
{
    public IReadOnlyList<Guid> PermissionIds { get; set; } = Array.Empty<Guid>();
}

public sealed class RoleFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
