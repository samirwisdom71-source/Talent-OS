using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class DevelopmentPlanFilterRequestValidator : AbstractValidator<DevelopmentPlanFilterRequest>
{
    public DevelopmentPlanFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
