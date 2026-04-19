using FluentValidation;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Identity.Validators;

public sealed class RoleFilterRequestValidator : AbstractValidator<RoleFilterRequest>
{
    public RoleFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
