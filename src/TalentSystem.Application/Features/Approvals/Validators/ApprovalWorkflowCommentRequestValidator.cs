using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class ApprovalWorkflowCommentRequestValidator : AbstractValidator<ApprovalWorkflowCommentRequest>
{
    public ApprovalWorkflowCommentRequestValidator()
    {
        RuleFor(x => x.Comments).MaximumLength(4000);
    }
}
