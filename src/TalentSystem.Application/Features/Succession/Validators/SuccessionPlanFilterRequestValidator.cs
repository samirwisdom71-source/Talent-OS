using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class SuccessionPlanFilterRequestValidator : AbstractValidator<SuccessionPlanFilterRequest>
{
    public SuccessionPlanFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
