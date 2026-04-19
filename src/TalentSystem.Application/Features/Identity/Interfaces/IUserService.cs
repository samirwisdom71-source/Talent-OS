using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> AssignRolesAsync(Guid id, AssignUserRolesRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<UserListItemDto>>> GetPagedAsync(UserFilterRequest request, CancellationToken cancellationToken = default);
}
