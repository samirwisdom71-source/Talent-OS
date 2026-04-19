using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class UpdateSuccessionPlanRequestValidator : AbstractValidator<UpdateSuccessionPlanRequest>
{
    public UpdateSuccessionPlanRequestValidator()
    {
        RuleFor(x => x.PlanName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
