using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Interfaces;

public interface IDevelopmentPlanService
{
    Task<Result<DevelopmentPlanDto>> CreateAsync(
        CreateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanDto>> UpdateAsync(
        Guid id,
        UpdateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<DevelopmentPlanDto>>> GetPagedAsync(
        DevelopmentPlanFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanDto>> ActivateAsync(
        Guid id,
        ActivateDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanDto>> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
