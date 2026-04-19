using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Abstractions;

namespace TalentSystem.Domain.Common;

public abstract class AuditableDomainEntity : AuditableEntity
{
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;

    public DateTime? DeletedOnUtc { get; set; }

    public Guid? DeletedByUserId { get; set; }
}
