namespace TalentSystem.Domain.Enums;

public enum NotificationType : byte
{
    ApprovalAssigned = 1,

    ApprovalSubmitted = 2,

    ApprovalApproved = 3,

    ApprovalRejected = 4,

    MarketplaceApplicationSubmitted = 10,

    MarketplaceApplicationAccepted = 11,

    MarketplaceApplicationRejected = 12,

    DevelopmentPlanActivated = 20,

    DevelopmentPlanCompleted = 21,

    General = 99
}
