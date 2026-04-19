using TalentSystem.Domain.Common;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Domain.Notifications;

public sealed class NotificationDispatchLog : AuditableDomainEntity
{
    public Guid NotificationId { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationDispatchStatus DispatchStatus { get; set; }

    public DateTime AttemptedOnUtc { get; set; }

    public string? ExternalReference { get; set; }

    public string? ErrorMessage { get; set; }

    public Notification Notification { get; set; } = null!;
}
