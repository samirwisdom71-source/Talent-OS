using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Notifications.DTOs;

public sealed class CreateNotificationRequest
{
    public Guid UserId { get; set; }

    public NotificationType NotificationType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }
}

public sealed class CreateNotificationBatchRequest
{
    public IReadOnlyList<Guid> UserIds { get; set; } = Array.Empty<Guid>();

    public NotificationType NotificationType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }
}

public sealed class NotificationDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public NotificationType NotificationType { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public NotificationChannel Channel { get; init; }

    public bool IsRead { get; init; }

    public DateTime? ReadOnUtc { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public string? RelatedEntityType { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}

public sealed class NotificationListItemDto
{
    public Guid Id { get; init; }

    public NotificationType NotificationType { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public NotificationChannel Channel { get; init; }

    public bool IsRead { get; init; }

    public Guid? RelatedEntityId { get; init; }

    public string? RelatedEntityType { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}

public sealed class NotificationFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public bool? UnreadOnly { get; set; }
}

public sealed class CreateNotificationTemplateRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? SubjectTemplate { get; set; }

    public string BodyTemplate { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    public NotificationChannel Channel { get; set; }
}

public sealed class UpdateNotificationTemplateRequest
{
    public string Name { get; set; } = string.Empty;

    public string? SubjectTemplate { get; set; }

    public string BodyTemplate { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    public NotificationChannel Channel { get; set; }

    public bool IsActive { get; set; }
}

public sealed class NotificationTemplateDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? SubjectTemplate { get; init; }

    public string BodyTemplate { get; init; } = string.Empty;

    public NotificationType NotificationType { get; init; }

    public NotificationChannel Channel { get; init; }

    public bool IsActive { get; init; }
}

public sealed class NotificationTemplateFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }

    public bool? ActiveOnly { get; set; }
}
