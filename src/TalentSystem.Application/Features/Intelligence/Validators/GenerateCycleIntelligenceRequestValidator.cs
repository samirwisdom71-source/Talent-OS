using FluentValidation;
using TalentSystem.Application.Features.Intelligence.DTOs;

namespace TalentSystem.Application.Features.Intelligence.Validators;

public sealed class GenerateCycleIntelligenceRequestValidator : AbstractValidator<GenerateCycleIntelligenceRequest>
{
    public GenerateCycleIntelligenceRequestValidator()
    {
        RuleFor(x => x.PerformanceCycleId).NotEmpty();
    }
}
