using FluentValidation;
using TalentSystem.Application.Features.Intelligence.DTOs;

namespace TalentSystem.Application.Features.Intelligence.Validators;

public sealed class GenerateEmployeeIntelligenceRequestValidator : AbstractValidator<GenerateEmployeeIntelligenceRequest>
{
    public GenerateEmployeeIntelligenceRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.PerformanceCycleId).NotEmpty();
        RuleFor(x => x.Target).IsInEnum();
    }
}
