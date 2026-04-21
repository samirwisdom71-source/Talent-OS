using FluentValidation;
using TalentSystem.Application.Features.Organizations.DTOs;

namespace TalentSystem.Application.Features.Organizations.Validators;

public sealed class UpdateOrganizationUnitRequestValidator : AbstractValidator<UpdateOrganizationUnitRequest>
{
    public UpdateOrganizationUnitRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(256);
    }
}
