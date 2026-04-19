using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Classification.Interfaces;

public interface IClassificationRuleSetService
{
    Task<Result<ClassificationRuleSetDto>> CreateAsync(
        CreateClassificationRuleSetRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ClassificationRuleSetDto>> UpdateAsync(
        Guid id,
        UpdateClassificationRuleSetRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ClassificationRuleSetDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ClassificationRuleSetDto>>> GetPagedAsync(
        ClassificationRuleSetFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<ClassificationRuleSetDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
