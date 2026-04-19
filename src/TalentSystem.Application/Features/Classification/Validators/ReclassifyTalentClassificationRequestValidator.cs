using FluentValidation;
using TalentSystem.Application.Features.Classification.DTOs;

namespace TalentSystem.Application.Features.Classification.Validators;

public sealed class ReclassifyTalentClassificationRequestValidator : AbstractValidator<ReclassifyTalentClassificationRequest>
{
    public ReclassifyTalentClassificationRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
