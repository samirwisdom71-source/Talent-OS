using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Performance.Interfaces;

public interface IPerformanceGoalService
{
    Task<Result<PerformanceGoalDto>> CreateAsync(
        CreatePerformanceGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceGoalDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceGoalRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceGoalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PerformanceGoalDto>>> GetPagedAsync(
        PerformanceGoalFilterRequest request,
        CancellationToken cancellationToken = default);
}
