using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Competencies;

public sealed class CompetencyLevel : AuditableDomainEntity
{
    public string Name { get; set; } = null!;

    public int NumericValue { get; set; }

    public string? Description { get; set; }

    public ICollection<JobCompetencyRequirement> JobRequirements { get; set; } = new List<JobCompetencyRequirement>();
}
