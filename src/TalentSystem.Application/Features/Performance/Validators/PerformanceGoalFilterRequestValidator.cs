using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class PerformanceGoalFilterRequestValidator : AbstractValidator<PerformanceGoalFilterRequest>
{
    public PerformanceGoalFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Search));
    }
}
