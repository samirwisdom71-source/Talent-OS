using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class CreateCompetencyCategoryRequestValidator : AbstractValidator<CreateCompetencyCategoryRequest>
{
    public CreateCompetencyCategoryRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
    }
}
