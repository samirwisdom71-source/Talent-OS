using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Competencies;

public sealed class Competency : AuditableDomainEntity
{
    public string Code { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public Guid CompetencyCategoryId { get; set; }

    public CompetencyCategory CompetencyCategory { get; set; } = null!;

    public ICollection<JobCompetencyRequirement> JobRequirements { get; set; } = new List<JobCompetencyRequirement>();
}
