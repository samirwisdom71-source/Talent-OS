using FluentValidation;
using TalentSystem.Application.Features.Identity.DTOs;

namespace TalentSystem.Application.Features.Identity.Validators;

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512);
    }
}
