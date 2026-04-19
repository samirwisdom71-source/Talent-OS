using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class UpdateCompetencyLevelRequestValidator : AbstractValidator<UpdateCompetencyLevelRequest>
{
    public UpdateCompetencyLevelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NumericValue).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
    }
}
