using FluentValidation;
using TalentSystem.Application.Features.Approvals.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Approvals.Validators;

public sealed class ApprovalRequestFilterRequestValidator : AbstractValidator<ApprovalRequestFilterRequest>
{
    public ApprovalRequestFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.RequestType).IsInEnum().When(x => x.RequestType.HasValue);
    }
}
