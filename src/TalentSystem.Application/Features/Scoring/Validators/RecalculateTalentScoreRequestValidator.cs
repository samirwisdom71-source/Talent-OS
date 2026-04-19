using FluentValidation;
using TalentSystem.Application.Features.Scoring.DTOs;

namespace TalentSystem.Application.Features.Scoring.Validators;

public sealed class RecalculateTalentScoreRequestValidator : AbstractValidator<RecalculateTalentScoreRequest>
{
    public RecalculateTalentScoreRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
