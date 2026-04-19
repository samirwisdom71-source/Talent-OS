namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class JobCompetencyRequirementDto
{
    public Guid Id { get; set; }

    public Guid PositionId { get; set; }

    public Guid CompetencyId { get; set; }

    public Guid RequiredLevelId { get; set; }
}
