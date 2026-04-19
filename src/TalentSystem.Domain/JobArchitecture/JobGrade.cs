using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.JobArchitecture;

public sealed class JobGrade : AuditableDomainEntity
{
    public string Name { get; set; } = null!;

    public int Level { get; set; }

    public ICollection<Position> Positions { get; set; } = new List<Position>();
}
