namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class CompetencyCategoryDto
{
    public Guid Id { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? Description { get; set; }
}
