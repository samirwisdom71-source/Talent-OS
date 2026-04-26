using FluentValidation;
using TalentSystem.Application.Features.Reports.DTOs;

namespace TalentSystem.Application.Features.Reports.Validators;

public sealed class SystemReportFilterRequestValidator : AbstractValidator<SystemReportFilterRequest>
{
    public SystemReportFilterRequestValidator()
    {
        RuleFor(x => x.ChartMonths)
            .InclusiveBetween(1, 24);

        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc.Value <= x.ToUtc.Value)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");

        RuleFor(x => x.Language)
            .Must(static lang => string.IsNullOrWhiteSpace(lang) || lang.Equals("ar", StringComparison.OrdinalIgnoreCase) || lang.Equals("en", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Language must be either 'ar' or 'en'.");
    }
}
