namespace TalentSystem.Domain.Enums;

public enum ApprovalRequestType : byte
{
    PerformanceEvaluation = 1,

    TalentClassification = 2,

    SuccessionPlan = 3,

    DevelopmentPlan = 4,

    MarketplaceOpportunity = 5,

    MarketplaceApplication = 6,

    Other = 99
}
