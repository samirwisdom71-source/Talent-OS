using FluentValidation;
using TalentSystem.Application.Features.JobArchitecture.DTOs;

namespace TalentSystem.Application.Features.JobArchitecture.Validators;

public sealed class CreatePositionRequestValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionRequestValidator()
    {
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.OrganizationUnitId).NotEmpty();
        RuleFor(x => x.JobGradeId).NotEmpty();
    }
}
