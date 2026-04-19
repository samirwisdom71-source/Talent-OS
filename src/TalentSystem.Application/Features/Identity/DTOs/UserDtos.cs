using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Identity.DTOs;

public sealed class UserDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public Guid? EmployeeId { get; init; }

    public DateTime? LastLoginUtc { get; init; }

    public IReadOnlyList<string> RoleNames { get; init; } = Array.Empty<string>();
}

public sealed class UserListItemDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public Guid? EmployeeId { get; init; }

    public DateTime? LastLoginUtc { get; init; }
}

public sealed class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }

    public IReadOnlyList<Guid> RoleIds { get; set; } = Array.Empty<Guid>();
}

public sealed class UpdateUserRequest
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }

    public string? NewPassword { get; set; }
}

public sealed class AssignUserRolesRequest
{
    public IReadOnlyList<Guid> RoleIds { get; set; } = Array.Empty<Guid>();
}

public sealed class UserFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
