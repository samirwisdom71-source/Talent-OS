using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Performance.Interfaces;

public interface IPerformanceCycleService
{
    Task<Result<PerformanceCycleDto>> CreateAsync(
        CreatePerformanceCycleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceCycleDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceCycleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceCycleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PerformanceCycleDto>>> GetPagedAsync(
        PerformanceCycleFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceCycleDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PerformanceCycleDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default);
}
