using TalentSystem.Domain.Common;
using TalentSystem.Domain.JobArchitecture;

namespace TalentSystem.Domain.Competencies;

public sealed class JobCompetencyRequirement : AuditableDomainEntity
{
    public Guid PositionId { get; set; }

    public Guid CompetencyId { get; set; }

    public Guid RequiredLevelId { get; set; }

    public Position Position { get; set; } = null!;

    public Competency Competency { get; set; } = null!;

    public CompetencyLevel RequiredLevel { get; set; } = null!;
}
