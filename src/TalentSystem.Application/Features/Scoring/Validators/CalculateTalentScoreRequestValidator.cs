using FluentValidation;
using TalentSystem.Application.Features.Scoring.DTOs;

namespace TalentSystem.Application.Features.Scoring.Validators;

public sealed class CalculateTalentScoreRequestValidator : AbstractValidator<CalculateTalentScoreRequest>
{
    public CalculateTalentScoreRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
