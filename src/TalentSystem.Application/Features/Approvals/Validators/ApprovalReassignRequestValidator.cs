using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class ApprovalReassignRequestValidator : AbstractValidator<ApprovalReassignRequest>
{
    public ApprovalReassignRequestValidator()
    {
        RuleFor(x => x.NewApproverUserId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
