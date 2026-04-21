using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Succession.Interfaces;

public interface ISuccessionPlanService
{
    Task<Result<SuccessionPlanDto>> CreateAsync(
        CreateSuccessionPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SuccessionPlanDto>> UpdateAsync(
        Guid id,
        UpdateSuccessionPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SuccessionPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<SuccessionPlanDto>>> GetPagedAsync(
        SuccessionPlanFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupItemDto>>> GetLookupAsync(
        SuccessionPlanLookupRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SuccessionPlanDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<SuccessionPlanDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default);
}
