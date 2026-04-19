using FluentValidation;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Marketplace.Validators;

public sealed class MarketplaceOpportunityFilterRequestValidator : AbstractValidator<MarketplaceOpportunityFilterRequest>
{
    public MarketplaceOpportunityFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);
        RuleFor(x => x.Status).Must(s => !s.HasValue || Enum.IsDefined(typeof(MarketplaceOpportunityStatus), s.Value));
        RuleFor(x => x.OpportunityType).Must(t => !t.HasValue || Enum.IsDefined(typeof(OpportunityType), t.Value));
        RuleFor(x => x.OrganizationUnitId).NotEqual(Guid.Empty).When(x => x.OrganizationUnitId.HasValue);
    }
}
