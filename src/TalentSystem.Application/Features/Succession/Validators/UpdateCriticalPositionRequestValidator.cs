using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class UpdateCriticalPositionRequestValidator : AbstractValidator<UpdateCriticalPositionRequest>
{
    public UpdateCriticalPositionRequestValidator()
    {
        RuleFor(x => x.CriticalityLevel).Must(e => Enum.IsDefined(typeof(CriticalityLevel), e));
        RuleFor(x => x.RiskLevel).Must(e => Enum.IsDefined(typeof(SuccessionRiskLevel), e));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
