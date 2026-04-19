using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Competencies;

public sealed class CompetencyCategory : AuditableDomainEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<Competency> Competencies { get; set; } = new List<Competency>();
}
