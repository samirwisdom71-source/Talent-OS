using FluentValidation;
using TalentSystem.Application.Features.Analytics.DTOs;

namespace TalentSystem.Application.Features.Analytics.Validators;

public sealed class TalentAnalyticsFilterRequestValidator : AbstractValidator<TalentAnalyticsFilterRequest>
{
    public TalentAnalyticsFilterRequestValidator()
    {
        RuleFor(x => x.PerformanceCycleId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("performanceCycleId must not be an empty GUID.");

        RuleFor(x => x.OrganizationUnitId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("organizationUnitId must not be an empty GUID.");
    }
}
