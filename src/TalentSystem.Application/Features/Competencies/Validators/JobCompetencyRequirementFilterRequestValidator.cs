using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class JobCompetencyRequirementFilterRequestValidator : AbstractValidator<JobCompetencyRequirementFilterRequest>
{
    public JobCompetencyRequirementFilterRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(PaginationConstants.MaxPageSize);

        RuleFor(x => x)
            .Must(x =>
                (x.PositionId.HasValue && x.PositionId.Value != Guid.Empty) ||
                (x.CompetencyId.HasValue && x.CompetencyId.Value != Guid.Empty))
            .WithMessage("Either positionId or competencyId must be provided.");
    }
}
