namespace TalentSystem.Application.Features.Intelligence.DTOs;

public sealed class IntelligenceGenerationResultDto
{
    public Guid? RunId { get; init; }

    public int InsightsGenerated { get; init; }

    public int RecommendationsGenerated { get; init; }
}
