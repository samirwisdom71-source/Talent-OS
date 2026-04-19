using FluentValidation;
using TalentSystem.Application.Features.Intelligence.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Intelligence.Validators;

public sealed class TalentInsightFilterRequestValidator : AbstractValidator<TalentInsightFilterRequest>
{
    public TalentInsightFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
