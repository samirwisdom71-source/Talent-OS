using FluentValidation;
using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Scoring.Validators;

public sealed class TalentScoreFilterRequestValidator : AbstractValidator<TalentScoreFilterRequest>
{
    public TalentScoreFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);

        When(
            x => x.MinFinalScore.HasValue && x.MaxFinalScore.HasValue,
            () =>
            {
                RuleFor(x => x).Must(x => x.MinFinalScore!.Value <= x.MaxFinalScore!.Value)
                    .WithMessage("minFinalScore must be less than or equal to maxFinalScore.");
            });
    }
}
