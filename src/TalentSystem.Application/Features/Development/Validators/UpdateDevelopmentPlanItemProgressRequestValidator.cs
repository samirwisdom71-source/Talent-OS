using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class UpdateDevelopmentPlanItemProgressRequestValidator : AbstractValidator<UpdateDevelopmentPlanItemProgressRequest>
{
    public UpdateDevelopmentPlanItemProgressRequestValidator()
    {
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0m, 100m);
    }
}
