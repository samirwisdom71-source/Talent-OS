using TalentSystem.Application.Features.Notifications;
using TalentSystem.Application.Features.Notifications.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Notifications.Integration;

public static class MarketplaceNotificationRequests
{
    public static CreateNotificationRequest ForApplicationAccepted(
        Guid applicationId,
        Guid userId,
        string opportunityTitle) =>
        new()
        {
            UserId = userId,
            NotificationType = NotificationType.MarketplaceApplicationAccepted,
            Title = "Application accepted",
            Message = $"Your application to \"{opportunityTitle}\" was accepted.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = applicationId,
            RelatedEntityType = NotificationRelatedEntityTypes.OpportunityApplication
        };

    public static CreateNotificationRequest ForApplicationRejected(
        Guid applicationId,
        Guid userId,
        string opportunityTitle) =>
        new()
        {
            UserId = userId,
            NotificationType = NotificationType.MarketplaceApplicationRejected,
            Title = "Application not successful",
            Message = $"Your application to \"{opportunityTitle}\" was not successful.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = applicationId,
            RelatedEntityType = NotificationRelatedEntityTypes.OpportunityApplication
        };
}
