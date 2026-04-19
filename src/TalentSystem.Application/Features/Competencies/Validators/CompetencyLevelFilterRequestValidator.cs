using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class CompetencyLevelFilterRequestValidator : AbstractValidator<CompetencyLevelFilterRequest>
{
    public CompetencyLevelFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Search));
    }
}
