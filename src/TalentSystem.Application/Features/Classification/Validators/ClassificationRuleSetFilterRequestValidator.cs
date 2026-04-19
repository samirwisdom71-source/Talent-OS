using FluentValidation;
using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Classification.Validators;

public sealed class ClassificationRuleSetFilterRequestValidator : AbstractValidator<ClassificationRuleSetFilterRequest>
{
    public ClassificationRuleSetFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Search));
    }
}
