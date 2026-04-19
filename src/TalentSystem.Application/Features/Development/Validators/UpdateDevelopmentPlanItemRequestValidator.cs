using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class UpdateDevelopmentPlanItemRequestValidator : AbstractValidator<UpdateDevelopmentPlanItemRequest>
{
    public UpdateDevelopmentPlanItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.ItemType).Must(t => Enum.IsDefined(typeof(DevelopmentItemType), t));
        RuleFor(x => x.Status).Must(s => Enum.IsDefined(typeof(DevelopmentItemStatus), s));
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0m, 100m);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x)
            .Must(x =>
                x.Status != DevelopmentItemStatus.Completed ||
                x.ProgressPercentage == 100m)
            .WithMessage("Completed items must have progress percentage of 100.");
    }
}
