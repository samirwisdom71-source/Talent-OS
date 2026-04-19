namespace TalentSystem.Domain.Enums;

public enum ApprovalRequestStatus : byte
{
    Draft = 1,

    Submitted = 2,

    InReview = 3,

    Approved = 4,

    Rejected = 5,

    Cancelled = 6
}
