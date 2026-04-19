using FluentValidation;
using TalentSystem.Application.Features.Potential.DTOs;

namespace TalentSystem.Application.Features.Potential.Validators;

public sealed class UpdatePotentialAssessmentRequestValidator : AbstractValidator<UpdatePotentialAssessmentRequest>
{
    public UpdatePotentialAssessmentRequestValidator()
    {
        RuleFor(x => x.AssessedByEmployeeId).NotEqual(Guid.Empty);

        RuleFor(x => x.AgilityScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.LeadershipScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.GrowthScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.MobilityScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);

        RuleFor(x => x.Comments).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.Comments));

        RuleForEach(x => x.Factors).SetValidator(new PotentialAssessmentFactorItemValidator());
    }
}
