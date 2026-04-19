using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class DevelopmentPlanItemFilterRequestValidator : AbstractValidator<DevelopmentPlanItemFilterRequest>
{
    public DevelopmentPlanItemFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.DevelopmentPlanId).NotEqual(Guid.Empty);
    }
}
