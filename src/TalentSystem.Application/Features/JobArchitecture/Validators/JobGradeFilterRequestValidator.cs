using FluentValidation;
using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.JobArchitecture.Validators;

public sealed class JobGradeFilterRequestValidator : AbstractValidator<JobGradeFilterRequest>
{
    public JobGradeFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(128).When(x => !string.IsNullOrWhiteSpace(x.Search));
        RuleFor(x => x.Level).GreaterThan(0).When(x => x.Level.HasValue);
    }
}
