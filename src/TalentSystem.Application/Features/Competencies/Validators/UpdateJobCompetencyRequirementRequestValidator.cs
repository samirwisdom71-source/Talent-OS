using FluentValidation;
using TalentSystem.Application.Features.Competencies.DTOs;

namespace TalentSystem.Application.Features.Competencies.Validators;

public sealed class UpdateJobCompetencyRequirementRequestValidator : AbstractValidator<UpdateJobCompetencyRequirementRequest>
{
    public UpdateJobCompetencyRequirementRequestValidator()
    {
        RuleFor(x => x.PositionId).NotEqual(Guid.Empty);
        RuleFor(x => x.CompetencyId).NotEqual(Guid.Empty);
        RuleFor(x => x.RequiredLevelId).NotEqual(Guid.Empty);
    }
}
