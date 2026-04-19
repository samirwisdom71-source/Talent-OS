using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class CreateSuccessionPlanRequestValidator : AbstractValidator<CreateSuccessionPlanRequest>
{
    public CreateSuccessionPlanRequestValidator()
    {
        RuleFor(x => x.CriticalPositionId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
