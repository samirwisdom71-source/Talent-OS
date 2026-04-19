using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Classification.Interfaces;

public interface ITalentClassificationService
{
    Task<Result<TalentClassificationDto>> ClassifyAsync(
        ClassifyTalentClassificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TalentClassificationDto>> ReclassifyAsync(
        ReclassifyTalentClassificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TalentClassificationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<TalentClassificationDto>> GetByEmployeeAndCycleAsync(
        Guid employeeId,
        Guid performanceCycleId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<TalentClassificationDto>>> GetPagedAsync(
        TalentClassificationFilterRequest request,
        CancellationToken cancellationToken = default);
}
