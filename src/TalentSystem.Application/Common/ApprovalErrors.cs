namespace TalentSystem.Application.Common;

public static class ApprovalErrors
{
    public const string NotFound = "Approval.NotFound";

    public const string DraftOnly = "Approval.DraftOnly";

    public const string InvalidStatus = "Approval.InvalidStatus";

    public const string InvalidStateForAssign = "Approval.InvalidStateForAssign";

    public const string NotRequester = "Approval.NotRequester";

    public const string NotCurrentApprover = "Approval.NotCurrentApprover";

    public const string ApproverNotFound = "Approval.ApproverNotFound";

    public const string ApproverInactive = "Approval.ApproverInactive";

    public const string CurrentUserRequired = "Approval.CurrentUserRequired";

    public const string RequestedUserNotFound = "Approval.RequestedUserNotFound";
}
