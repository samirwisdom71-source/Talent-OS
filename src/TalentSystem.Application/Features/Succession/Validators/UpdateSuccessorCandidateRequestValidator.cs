using FluentValidation;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.Validators;

public sealed class UpdateSuccessorCandidateRequestValidator : AbstractValidator<UpdateSuccessorCandidateRequest>
{
    public UpdateSuccessorCandidateRequestValidator()
    {
        RuleFor(x => x.ReadinessLevel).Must(e => Enum.IsDefined(typeof(ReadinessLevel), e));
        RuleFor(x => x.RankOrder).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
