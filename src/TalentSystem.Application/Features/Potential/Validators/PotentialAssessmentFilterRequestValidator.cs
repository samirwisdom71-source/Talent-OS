using FluentValidation;
using TalentSystem.Application.Features.Potential.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Potential.Validators;

public sealed class PotentialAssessmentFilterRequestValidator : AbstractValidator<PotentialAssessmentFilterRequest>
{
    public PotentialAssessmentFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
