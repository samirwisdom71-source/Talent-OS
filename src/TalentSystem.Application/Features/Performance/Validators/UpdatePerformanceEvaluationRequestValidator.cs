using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class UpdatePerformanceEvaluationRequestValidator : AbstractValidator<UpdatePerformanceEvaluationRequest>
{
    public UpdatePerformanceEvaluationRequestValidator()
    {
        RuleFor(x => x.OverallScore).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.ManagerComments).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.ManagerComments));
        RuleFor(x => x.EmployeeComments).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.EmployeeComments));
    }
}
