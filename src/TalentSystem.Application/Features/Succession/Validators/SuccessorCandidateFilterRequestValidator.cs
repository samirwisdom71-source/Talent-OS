using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class SuccessorCandidateFilterRequestValidator : AbstractValidator<SuccessorCandidateFilterRequest>
{
    public SuccessorCandidateFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.SuccessionPlanId).NotEqual(Guid.Empty);
    }
}
