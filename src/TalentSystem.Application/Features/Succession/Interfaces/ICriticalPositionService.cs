using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Succession.Interfaces;

public interface ICriticalPositionService
{
    Task<Result<CriticalPositionDto>> CreateAsync(
        CreateCriticalPositionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CriticalPositionDto>> UpdateAsync(
        Guid id,
        UpdateCriticalPositionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CriticalPositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<CriticalPositionDto>>> GetPagedAsync(
        CriticalPositionFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CriticalPositionDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
