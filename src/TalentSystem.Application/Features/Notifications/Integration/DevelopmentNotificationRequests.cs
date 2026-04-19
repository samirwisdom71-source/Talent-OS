using TalentSystem.Application.Features.Notifications;
using TalentSystem.Application.Features.Notifications.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Notifications.Integration;

public static class DevelopmentNotificationRequests
{
    public static CreateNotificationRequest ForPlanActivated(Guid planId, Guid userId, string planTitle) =>
        new()
        {
            UserId = userId,
            NotificationType = NotificationType.DevelopmentPlanActivated,
            Title = "Development plan activated",
            Message = $"Your development plan \"{planTitle}\" is now active.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = planId,
            RelatedEntityType = NotificationRelatedEntityTypes.DevelopmentPlan
        };

    public static CreateNotificationRequest ForPlanCompleted(Guid planId, Guid userId, string planTitle) =>
        new()
        {
            UserId = userId,
            NotificationType = NotificationType.DevelopmentPlanCompleted,
            Title = "Development plan completed",
            Message = $"Your development plan \"{planTitle}\" has been marked completed.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = planId,
            RelatedEntityType = NotificationRelatedEntityTypes.DevelopmentPlan
        };
}
