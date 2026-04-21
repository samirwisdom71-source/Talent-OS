using FluentValidation;
using TalentSystem.Application.Features.Organizations.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Organizations.Validators;

public sealed class OrganizationUnitFilterRequestValidator : AbstractValidator<OrganizationUnitFilterRequest>
{
    public OrganizationUnitFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
