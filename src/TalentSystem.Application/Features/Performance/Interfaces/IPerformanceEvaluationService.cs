using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Performance.Interfaces;

public interface IPerformanceEvaluationService
{
    Task<Result<PerformanceEvaluationDto>> CreateAsync(
        CreatePerformanceEvaluationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceEvaluationDto>> UpdateAsync(
        Guid id,
        UpdatePerformanceEvaluationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PerformanceEvaluationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PerformanceEvaluationDto>>> GetPagedAsync(
        PerformanceEvaluationFilterRequest request,
        CancellationToken cancellationToken = default);
}
