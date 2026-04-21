using FluentValidation;
using TalentSystem.Application.Features.Organizations.DTOs;

namespace TalentSystem.Application.Features.Organizations.Validators;

public sealed class CreateOrganizationUnitRequestValidator : AbstractValidator<CreateOrganizationUnitRequest>
{
    public CreateOrganizationUnitRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(256);
    }
}
