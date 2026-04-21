using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.JobArchitecture.Interfaces;

public interface IPositionService
{
    Task<Result<PositionDto>> CreateAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PositionDto>> UpdateAsync(
        Guid id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PositionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PositionDto>>> GetPagedAsync(
        PositionFilterRequest request,
        CancellationToken cancellationToken = default);
}
