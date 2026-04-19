namespace TalentSystem.Application.Features.Identity;

internal static class PermissionDefinitions
{
    internal sealed record Definition(string Code, string Name, string Module);

    internal static readonly IReadOnlyList<Definition> All =
    [
        new(PermissionCodes.UserManage, "Manage users", "Identity"),
        new(PermissionCodes.RoleManage, "Manage roles and permissions", "Identity"),
        new(PermissionCodes.EmployeeView, "View employees", "Employee"),
        new(PermissionCodes.EmployeeEdit, "Create and update employees", "Employee"),
        new(PermissionCodes.CompetencyView, "View competencies", "Competency"),
        new(PermissionCodes.CompetencyEdit, "Manage competencies", "Competency"),
        new(PermissionCodes.PerformanceView, "View performance data", "Performance"),
        new(PermissionCodes.PerformanceManage, "Manage performance cycles and evaluations", "Performance"),
        new(PermissionCodes.PotentialView, "View potential assessments", "Potential"),
        new(PermissionCodes.PotentialManage, "Manage potential assessments", "Potential"),
        new(PermissionCodes.ScoringView, "View talent scores", "Scoring"),
        new(PermissionCodes.ScoringManage, "Manage scoring policies and calculations", "Scoring"),
        new(PermissionCodes.ClassificationView, "View talent classifications", "Classification"),
        new(PermissionCodes.ClassificationManage, "Manage classifications and rule sets", "Classification"),
        new(PermissionCodes.SuccessionView, "View succession data", "Succession"),
        new(PermissionCodes.SuccessionManage, "Manage succession plans and candidates", "Succession"),
        new(PermissionCodes.DevelopmentView, "View development plans", "Development"),
        new(PermissionCodes.DevelopmentManage, "Manage development plans", "Development"),
        new(PermissionCodes.MarketplaceView, "View marketplace opportunities", "Marketplace"),
        new(PermissionCodes.MarketplaceManage, "Manage marketplace opportunities", "Marketplace"),
        new(PermissionCodes.MarketplaceApply, "Apply to internal opportunities", "Marketplace"),
        new(PermissionCodes.AnalyticsView, "View analytics dashboards", "Analytics"),
        new(PermissionCodes.ApprovalRequestCreate, "Create and manage draft approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestView, "View approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestAssign, "Assign or reassign approvers", "Approvals"),
        new(PermissionCodes.ApprovalRequestReview, "Start review on approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestApprove, "Approve approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestReject, "Reject approval requests", "Approvals"),
        new(PermissionCodes.NotificationView, "View notification templates and any notification record", "Notifications"),
        new(PermissionCodes.NotificationManage, "Create notifications and manage templates", "Notifications"),
        new(PermissionCodes.IntelligenceView, "View talent insights and recommendations", "Intelligence"),
        new(PermissionCodes.IntelligenceGenerate, "Run intelligence generation for employees or cycles", "Intelligence"),
        new(PermissionCodes.IntelligenceManage, "Dismiss or accept intelligence recommendations", "Intelligence")
    ];
}
