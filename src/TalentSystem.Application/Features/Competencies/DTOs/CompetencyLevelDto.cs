namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class CompetencyLevelDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int NumericValue { get; set; }

    public string? Description { get; set; }
}
