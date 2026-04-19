using FluentValidation;
using TalentSystem.Application.Features.Intelligence.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Intelligence.Validators;

public sealed class TalentRecommendationFilterRequestValidator : AbstractValidator<TalentRecommendationFilterRequest>
{
    public TalentRecommendationFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
