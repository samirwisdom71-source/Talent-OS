using FluentValidation;
using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Scoring.Validators;

public sealed class ScoringPolicyFilterRequestValidator : AbstractValidator<ScoringPolicyFilterRequest>
{
    public ScoringPolicyFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Search));
    }
}
