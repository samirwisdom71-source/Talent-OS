using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
