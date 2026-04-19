using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class UpdatePerformanceGoalRequestValidator : AbstractValidator<UpdatePerformanceGoalRequest>
{
    public UpdatePerformanceGoalRequestValidator()
    {
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.Weight).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.TargetValue).MaximumLength(512).When(x => !string.IsNullOrEmpty(x.TargetValue));
    }
}
