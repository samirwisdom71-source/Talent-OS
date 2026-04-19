namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class CreateCompetencyLevelRequest
{
    public string Name { get; set; } = string.Empty;

    public int NumericValue { get; set; }

    public string? Description { get; set; }
}
