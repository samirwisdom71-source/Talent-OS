using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Identity;

namespace TalentSystem.Domain.Approvals;

public sealed class ApprovalRequest : AuditableDomainEntity
{
    public ApprovalRequestType RequestType { get; set; }

    public Guid RelatedEntityId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public Guid? CurrentApproverUserId { get; set; }

    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Draft;

    public DateTime? SubmittedOnUtc { get; set; }

    public DateTime? CompletedOnUtc { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Notes { get; set; }

    public User RequestedByUser { get; set; } = null!;

    public User? CurrentApproverUser { get; set; }

    public ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();

    public ICollection<ApprovalAssignment> Assignments { get; set; } = new List<ApprovalAssignment>();
}
