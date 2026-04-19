using FluentValidation;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Marketplace.Validators;

public sealed class OpportunityApplicationFilterRequestValidator : AbstractValidator<OpportunityApplicationFilterRequest>
{
    public OpportunityApplicationFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);

        RuleFor(x => x)
            .Must(x =>
                (x.MarketplaceOpportunityId.HasValue && x.MarketplaceOpportunityId.Value != Guid.Empty) ||
                (x.EmployeeId.HasValue && x.EmployeeId.Value != Guid.Empty))
            .WithMessage("Either marketplaceOpportunityId or employeeId must be provided.");
    }
}
