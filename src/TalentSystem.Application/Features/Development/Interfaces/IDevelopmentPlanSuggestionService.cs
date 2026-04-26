using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Development.Interfaces;

public interface IDevelopmentPlanSuggestionService
{
    Task<Result<DevelopmentPlanSuggestionDto>> SuggestAsync(
        SuggestDevelopmentPlanRequest request,
        CancellationToken cancellationToken = default);
}
