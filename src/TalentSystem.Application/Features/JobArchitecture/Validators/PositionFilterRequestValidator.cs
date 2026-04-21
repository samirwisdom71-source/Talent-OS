using FluentValidation;
using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.JobArchitecture.Validators;

public sealed class PositionFilterRequestValidator : AbstractValidator<PositionFilterRequest>
{
    public PositionFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
