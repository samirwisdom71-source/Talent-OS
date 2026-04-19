using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class UpdateApprovalRequestRequestValidator : AbstractValidator<UpdateApprovalRequestRequest>
{
    public UpdateApprovalRequestRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Summary).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}
