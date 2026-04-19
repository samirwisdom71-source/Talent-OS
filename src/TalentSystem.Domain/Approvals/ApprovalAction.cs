using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Identity;

namespace TalentSystem.Domain.Approvals;

public sealed class ApprovalAction : AuditableDomainEntity
{
    public Guid ApprovalRequestId { get; set; }

    public Guid ActionByUserId { get; set; }

    public ApprovalActionType ActionType { get; set; }

    public string? Comments { get; set; }

    public DateTime ActionedOnUtc { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;

    public User ActionByUser { get; set; } = null!;
}
