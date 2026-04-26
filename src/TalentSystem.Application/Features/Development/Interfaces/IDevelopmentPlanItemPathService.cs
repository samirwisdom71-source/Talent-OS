using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Interfaces;

public interface IDevelopmentPlanItemPathService
{
    Task<Result<DevelopmentPlanItemPathDto>> AddAsync(
        Guid planItemId,
        CreateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemPathDto>> UpdateAsync(
        Guid pathId,
        UpdateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemPathDto>> GetByIdAsync(Guid pathId, CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(Guid pathId, CancellationToken cancellationToken = default);

    Task<Result<DevelopmentPlanItemPathDto>> MarkCompletedAsync(Guid pathId, CancellationToken cancellationToken = default);
}
