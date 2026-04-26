using FluentValidation;
using TalentSystem.Application.Features.Analytics.DTOs;

namespace TalentSystem.Application.Features.Analytics.Validators;

public sealed class AnalyticsDateRangeFilterValidator : AbstractValidator<AnalyticsDateRangeFilter>
{
    public AnalyticsDateRangeFilterValidator()
    {
        RuleFor(x => x)
            .Must(f => (!f.FromUtc.HasValue && !f.ToUtc.HasValue) || (f.FromUtc.HasValue && f.ToUtc.HasValue))
            .WithMessage("Both fromUtc and toUtc are required when filtering by date.");

        RuleFor(x => x)
            .Must(f => !f.FromUtc.HasValue || !f.ToUtc.HasValue || f.FromUtc.Value <= f.ToUtc.Value)
            .WithMessage("fromUtc must be less than or equal to toUtc.");
    }
}
