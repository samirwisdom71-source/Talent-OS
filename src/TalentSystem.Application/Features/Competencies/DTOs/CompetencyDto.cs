namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class CompetencyDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CompetencyCategoryId { get; set; }
}
