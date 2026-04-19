using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Interfaces;

public interface IDevelopmentPlanItemService
{
    Task<Result<DevelopmentPlanItemDto>> AddAsync(
        CreateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemDto>> UpdateAsync(
        Guid id,
        UpdateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<DevelopmentPlanItemDto>>> GetPagedAsync(
        DevelopmentPlanItemFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemDto>> UpdateProgressAsync(
        Guid id,
        UpdateDevelopmentPlanItemProgressRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemDto>> MarkCompletedAsync(Guid id, CancellationToken cancellationToken = default);
}
