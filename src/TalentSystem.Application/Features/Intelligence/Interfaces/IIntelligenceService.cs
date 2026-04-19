using TalentSystem.Application.Features.Intelligence.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Intelligence.Interfaces;

public interface IIntelligenceService
{
    Task<Result<IntelligenceGenerationResultDto>> GenerateInsightsForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IntelligenceGenerationResultDto>> GenerateRecommendationsForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IntelligenceGenerationResultDto>> GenerateAllForEmployeeAsync(
        GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IntelligenceGenerationResultDto>> GenerateForPerformanceCycleAsync(
        GenerateCycleIntelligenceRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TalentInsightDto>> GetInsightByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<TalentRecommendationDto>> GetRecommendationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<TalentInsightDto>>> GetPagedInsightsAsync(
        TalentInsightFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<TalentRecommendationDto>>> GetPagedRecommendationsAsync(
        TalentRecommendationFilterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DismissInsightAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> DismissRecommendationAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> AcceptRecommendationAsync(Guid id, CancellationToken cancellationToken = default);
}
