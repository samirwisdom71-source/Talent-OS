using TalentSystem.Domain.Common;
using TalentSystem.Domain.Identity;

namespace TalentSystem.Domain.Approvals;

public sealed class ApprovalAssignment : AuditableDomainEntity
{
    public Guid ApprovalRequestId { get; set; }

    public Guid AssignedToUserId { get; set; }

    public Guid AssignedByUserId { get; set; }

    public DateTime AssignedOnUtc { get; set; }

    public bool IsCurrent { get; set; }

    public string? Notes { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;

    public User AssignedToUser { get; set; } = null!;

    public User AssignedByUser { get; set; } = null!;
}
