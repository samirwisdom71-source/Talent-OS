using FluentValidation;
using TalentSystem.Application.Features.Notifications.DTOs;

namespace TalentSystem.Application.Features.Notifications.Validators;

public sealed class UpdateNotificationTemplateRequestValidator : AbstractValidator<UpdateNotificationTemplateRequest>
{
    public UpdateNotificationTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SubjectTemplate).MaximumLength(512);
        RuleFor(x => x.BodyTemplate).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.NotificationType).IsInEnum();
        RuleFor(x => x.Channel).IsInEnum();
    }
}
