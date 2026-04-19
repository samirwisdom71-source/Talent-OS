using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class ActivateDevelopmentPlanRequestValidator : AbstractValidator<ActivateDevelopmentPlanRequest>
{
    public ActivateDevelopmentPlanRequestValidator()
    {
        RuleFor(x => x.ApprovedByEmployeeId)
            .NotEqual(Guid.Empty)
            .When(x => x.ApprovedByEmployeeId.HasValue);
    }
}
