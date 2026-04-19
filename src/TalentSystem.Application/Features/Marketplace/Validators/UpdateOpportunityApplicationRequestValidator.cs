using FluentValidation;
using TalentSystem.Application.Features.Marketplace.DTOs;

namespace TalentSystem.Application.Features.Marketplace.Validators;

public sealed class UpdateOpportunityApplicationRequestValidator : AbstractValidator<UpdateOpportunityApplicationRequest>
{
    public UpdateOpportunityApplicationRequestValidator()
    {
        RuleFor(x => x.MotivationStatement).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.MotivationStatement));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
