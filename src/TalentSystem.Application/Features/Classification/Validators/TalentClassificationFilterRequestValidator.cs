using FluentValidation;
using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Classification.Validators;

public sealed class TalentClassificationFilterRequestValidator : AbstractValidator<TalentClassificationFilterRequest>
{
    public TalentClassificationFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
    }
}
