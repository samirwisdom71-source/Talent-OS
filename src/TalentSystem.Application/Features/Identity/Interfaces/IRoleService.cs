using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Interfaces;

public interface IRoleService
{
    Task<Result<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> AssignPermissionsAsync(Guid id, AssignRolePermissionsRequest request, CancellationToken cancellationToken = default);

    Task<Result<RoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<RoleListItemDto>>> GetPagedAsync(RoleFilterRequest request, CancellationToken cancellationToken = default);
}
