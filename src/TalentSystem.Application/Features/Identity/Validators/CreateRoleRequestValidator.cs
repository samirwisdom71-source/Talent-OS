using FluentValidation;
using TalentSystem.Application.Features.Identity.DTOs;

namespace TalentSystem.Application.Features.Identity.Validators;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512);
    }
}
