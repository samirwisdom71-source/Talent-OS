using FluentValidation;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Marketplace.Validators;

public sealed class UpdateMarketplaceOpportunityRequestValidator : AbstractValidator<UpdateMarketplaceOpportunityRequest>
{
    public UpdateMarketplaceOpportunityRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(4000).When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.OpportunityType).Must(t => Enum.IsDefined(typeof(OpportunityType), t));
        RuleFor(x => x.OrganizationUnitId).NotEqual(Guid.Empty);
        RuleFor(x => x.PositionId).NotEqual(Guid.Empty).When(x => x.PositionId.HasValue);
        RuleFor(x => x.RequiredCompetencySummary).MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.RequiredCompetencySummary));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
        RuleFor(x => x.MaxApplicants).GreaterThan(0).When(x => x.MaxApplicants.HasValue);

        RuleFor(x => x)
            .Must(x => !x.CloseDate.HasValue || x.OpenDate <= x.CloseDate.Value)
            .WithMessage("OpenDate must be on or before CloseDate when CloseDate is provided.");
    }
}
