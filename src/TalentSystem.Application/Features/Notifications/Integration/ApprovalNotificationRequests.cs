using TalentSystem.Application.Features.Notifications;
using TalentSystem.Application.Features.Notifications.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Notifications.Integration;

public static class ApprovalNotificationRequests
{
    public static CreateNotificationRequest ForAssigned(Guid approvalRequestId, Guid approverUserId, string requestTitle) =>
        new()
        {
            UserId = approverUserId,
            NotificationType = NotificationType.ApprovalAssigned,
            Title = "Approval assignment",
            Message = $"You were assigned as approver for: {requestTitle}.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = approvalRequestId,
            RelatedEntityType = NotificationRelatedEntityTypes.ApprovalRequest
        };

    public static CreateNotificationRequest ForApproved(Guid approvalRequestId, Guid requesterUserId, string requestTitle) =>
        new()
        {
            UserId = requesterUserId,
            NotificationType = NotificationType.ApprovalApproved,
            Title = "Approval approved",
            Message = $"Your approval request \"{requestTitle}\" was approved.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = approvalRequestId,
            RelatedEntityType = NotificationRelatedEntityTypes.ApprovalRequest
        };

    public static CreateNotificationRequest ForRejected(Guid approvalRequestId, Guid requesterUserId, string requestTitle) =>
        new()
        {
            UserId = requesterUserId,
            NotificationType = NotificationType.ApprovalRejected,
            Title = "Approval rejected",
            Message = $"Your approval request \"{requestTitle}\" was rejected.",
            Channel = NotificationChannel.InApp,
            RelatedEntityId = approvalRequestId,
            RelatedEntityType = NotificationRelatedEntityTypes.ApprovalRequest
        };
}
