using FluentValidation;
using TalentSystem.Application.Features.Scoring.DTOs;

namespace TalentSystem.Application.Features.Scoring.Validators;

public sealed class UpdateScoringPolicyRequestValidator : AbstractValidator<UpdateScoringPolicyRequest>
{
    public UpdateScoringPolicyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PerformanceWeight).GreaterThan(0);
        RuleFor(x => x.PotentialWeight).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x)
            .Must(x => x.PerformanceWeight + x.PotentialWeight == 100m)
            .WithMessage("Performance and potential weights must sum to 100.");
    }
}
