using FluentValidation;
using TalentSystem.Application.Features.Marketplace.DTOs;

namespace TalentSystem.Application.Features.Marketplace.Validators;

public sealed class ApplyOpportunityRequestValidator : AbstractValidator<ApplyOpportunityRequest>
{
    public ApplyOpportunityRequestValidator()
    {
        RuleFor(x => x.MarketplaceOpportunityId).NotEqual(Guid.Empty);
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.MotivationStatement).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.MotivationStatement));
    }
}
