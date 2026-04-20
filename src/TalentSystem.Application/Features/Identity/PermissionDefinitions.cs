namespace TalentSystem.Application.Features.Identity;

internal static class PermissionDefinitions
{
    internal sealed record Definition(
        string Code,
        string NameAr,
        string NameEn,
        string? DescriptionAr,
        string? DescriptionEn,
        string Module);

    internal static readonly IReadOnlyList<Definition> All =
    [
        new(PermissionCodes.UserManage, "إدارة المستخدمين", "Manage users", null, "Manage users", "Identity"),
        new(PermissionCodes.RoleManage, "إدارة الأدوار والصلاحيات", "Manage roles and permissions", null, "Manage roles and permissions", "Identity"),
        new(PermissionCodes.EmployeeView, "عرض الموظفين", "View employees", null, "View employees", "Employee"),
        new(PermissionCodes.EmployeeEdit, "إنشاء وتحديث الموظفين", "Create and update employees", null, "Create and update employees", "Employee"),
        new(PermissionCodes.CompetencyView, "عرض الكفاءات", "View competencies", null, "View competencies", "Competency"),
        new(PermissionCodes.CompetencyEdit, "إدارة الكفاءات", "Manage competencies", null, "Manage competencies", "Competency"),
        new(PermissionCodes.PerformanceView, "عرض بيانات الأداء", "View performance data", null, "View performance data", "Performance"),
        new(PermissionCodes.PerformanceManage, "إدارة دورات وتقييمات الأداء", "Manage performance cycles and evaluations", null, "Manage performance cycles and evaluations", "Performance"),
        new(PermissionCodes.PotentialView, "عرض تقييمات الإمكانات", "View potential assessments", null, "View potential assessments", "Potential"),
        new(PermissionCodes.PotentialManage, "إدارة تقييمات الإمكانات", "Manage potential assessments", null, "Manage potential assessments", "Potential"),
        new(PermissionCodes.ScoringView, "عرض درجات المواهب", "View talent scores", null, "View talent scores", "Scoring"),
        new(PermissionCodes.ScoringManage, "إدارة سياسات وحسابات الدرجات", "Manage scoring policies and calculations", null, "Manage scoring policies and calculations", "Scoring"),
        new(PermissionCodes.ClassificationView, "عرض تصنيفات المواهب", "View talent classifications", null, "View talent classifications", "Classification"),
        new(PermissionCodes.ClassificationManage, "إدارة التصنيفات ومجموعات القواعد", "Manage classifications and rule sets", null, "Manage classifications and rule sets", "Classification"),
        new(PermissionCodes.SuccessionView, "عرض بيانات الإحلال الوظيفي", "View succession data", null, "View succession data", "Succession"),
        new(PermissionCodes.SuccessionManage, "إدارة خطط ومرشحي الإحلال", "Manage succession plans and candidates", null, "Manage succession plans and candidates", "Succession"),
        new(PermissionCodes.DevelopmentView, "عرض خطط التطوير", "View development plans", null, "View development plans", "Development"),
        new(PermissionCodes.DevelopmentManage, "إدارة خطط التطوير", "Manage development plans", null, "Manage development plans", "Development"),
        new(PermissionCodes.MarketplaceView, "عرض فرص السوق الداخلي", "View marketplace opportunities", null, "View marketplace opportunities", "Marketplace"),
        new(PermissionCodes.MarketplaceManage, "إدارة فرص السوق الداخلي", "Manage marketplace opportunities", null, "Manage marketplace opportunities", "Marketplace"),
        new(PermissionCodes.MarketplaceApply, "التقديم على الفرص الداخلية", "Apply to internal opportunities", null, "Apply to internal opportunities", "Marketplace"),
        new(PermissionCodes.AnalyticsView, "عرض لوحات التحليلات", "View analytics dashboards", null, "View analytics dashboards", "Analytics"),
        new(PermissionCodes.ApprovalRequestCreate, "إنشاء وإدارة طلبات الموافقة المبدئية", "Create and manage draft approval requests", null, "Create and manage draft approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestView, "عرض طلبات الموافقة", "View approval requests", null, "View approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestAssign, "تعيين أو إعادة تعيين المعتمدين", "Assign or reassign approvers", null, "Assign or reassign approvers", "Approvals"),
        new(PermissionCodes.ApprovalRequestReview, "بدء مراجعة طلبات الموافقة", "Start review on approval requests", null, "Start review on approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestApprove, "اعتماد طلبات الموافقة", "Approve approval requests", null, "Approve approval requests", "Approvals"),
        new(PermissionCodes.ApprovalRequestReject, "رفض طلبات الموافقة", "Reject approval requests", null, "Reject approval requests", "Approvals"),
        new(PermissionCodes.NotificationView, "عرض قوالب وسجلات الإشعارات", "View notification templates and any notification record", null, "View notification templates and any notification record", "Notifications"),
        new(PermissionCodes.NotificationManage, "إنشاء الإشعارات وإدارة القوالب", "Create notifications and manage templates", null, "Create notifications and manage templates", "Notifications"),
        new(PermissionCodes.IntelligenceView, "عرض الرؤى والتوصيات", "View talent insights and recommendations", null, "View talent insights and recommendations", "Intelligence"),
        new(PermissionCodes.IntelligenceGenerate, "تشغيل توليد الرؤى للموظفين أو الدورات", "Run intelligence generation for employees or cycles", null, "Run intelligence generation for employees or cycles", "Intelligence"),
        new(PermissionCodes.IntelligenceManage, "قبول أو رفض توصيات الرؤى", "Dismiss or accept intelligence recommendations", null, "Dismiss or accept intelligence recommendations", "Intelligence")
    ];
}
