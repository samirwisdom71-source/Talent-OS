using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Interfaces;

public interface IPermissionService
{
    Task<Result> SeedDefaultPermissionsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
}
