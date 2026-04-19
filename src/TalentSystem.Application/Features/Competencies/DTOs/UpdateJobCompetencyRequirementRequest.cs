namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class UpdateJobCompetencyRequirementRequest
{
    public Guid PositionId { get; set; }

    public Guid CompetencyId { get; set; }

    public Guid RequiredLevelId { get; set; }
}
