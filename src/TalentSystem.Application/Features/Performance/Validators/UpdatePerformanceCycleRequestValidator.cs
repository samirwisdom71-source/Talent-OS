using FluentValidation;
using TalentSystem.Application.Features.Performance.DTOs;

namespace TalentSystem.Application.Features.Performance.Validators;

public sealed class UpdatePerformanceCycleRequestValidator : AbstractValidator<UpdatePerformanceCycleRequest>
{
    public UpdatePerformanceCycleRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}
