using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class UpdateCompetencyRequestValidator : AbstractValidator<UpdateCompetencyRequest>
{
    public UpdateCompetencyRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.CompetencyCategoryId).NotEqual(Guid.Empty);
    }
}
