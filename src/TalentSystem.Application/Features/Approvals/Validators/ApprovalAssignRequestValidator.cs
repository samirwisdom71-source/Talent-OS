using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class ApprovalAssignRequestValidator : AbstractValidator<ApprovalAssignRequest>
{
    public ApprovalAssignRequestValidator()
    {
        RuleFor(x => x.ApproverUserId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
