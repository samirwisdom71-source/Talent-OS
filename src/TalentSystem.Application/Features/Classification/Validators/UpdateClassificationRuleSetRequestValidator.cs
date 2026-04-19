using FluentValidation;
using TalentSystem.Application.Features.Classification.DTOs;

namespace TalentSystem.Application.Features.Classification.Validators;

public sealed class UpdateClassificationRuleSetRequestValidator : AbstractValidator<UpdateClassificationRuleSetRequest>
{
    public UpdateClassificationRuleSetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.LowThreshold).GreaterThanOrEqualTo(0m).LessThanOrEqualTo(100m);
        RuleFor(x => x.HighThreshold).GreaterThanOrEqualTo(0m).LessThanOrEqualTo(100m);

        RuleFor(x => x)
            .Must(x => x.LowThreshold < x.HighThreshold)
            .WithMessage("LowThreshold must be less than HighThreshold.");

        RuleFor(x => x)
            .Must(x => x.HighThreshold <= 100m)
            .WithMessage("HighThreshold must be less than or equal to 100.");
    }
}
