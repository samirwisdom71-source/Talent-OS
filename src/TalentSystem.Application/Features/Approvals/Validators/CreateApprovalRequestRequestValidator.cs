using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class CreateApprovalRequestRequestValidator : AbstractValidator<CreateApprovalRequestRequest>
{
    public CreateApprovalRequestRequestValidator()
    {
        RuleFor(x => x.RelatedEntityId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Summary).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.RequestType).IsInEnum();
    }
}
