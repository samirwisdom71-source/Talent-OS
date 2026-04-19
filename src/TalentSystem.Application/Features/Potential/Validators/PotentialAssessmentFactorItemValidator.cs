using FluentValidation;
using TalentSystem.Application.Features.Potential.DTOs;

namespace TalentSystem.Application.Features.Potential.Validators;

public sealed class PotentialAssessmentFactorItemValidator : AbstractValidator<PotentialAssessmentFactorItemDto>
{
    public PotentialAssessmentFactorItemValidator()
    {
        RuleFor(x => x.FactorName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Weight).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
