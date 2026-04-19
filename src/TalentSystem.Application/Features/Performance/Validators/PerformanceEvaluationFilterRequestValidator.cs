using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class PerformanceEvaluationFilterRequestValidator : AbstractValidator<PerformanceEvaluationFilterRequest>
{
    public PerformanceEvaluationFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
