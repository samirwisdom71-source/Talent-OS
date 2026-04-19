using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class CreatePerformanceGoalRequestValidator : AbstractValidator<CreatePerformanceGoalRequest>
{
    public CreatePerformanceGoalRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.Weight).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.TargetValue).MaximumLength(512).When(x => !string.IsNullOrEmpty(x.TargetValue));

        RuleFor(x => x.Status)
            .Must(s => s is PerformanceGoalStatus.Draft or PerformanceGoalStatus.Active)
            .WithMessage("New goals must be created as draft or active.");
    }
}
