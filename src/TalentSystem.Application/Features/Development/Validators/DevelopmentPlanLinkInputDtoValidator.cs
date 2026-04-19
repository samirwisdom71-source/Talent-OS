using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class DevelopmentPlanLinkInputDtoValidator : AbstractValidator<DevelopmentPlanLinkInputDto>
{
    public DevelopmentPlanLinkInputDtoValidator()
    {
        RuleFor(x => x.LinkType).Must(t => Enum.IsDefined(typeof(DevelopmentPlanLinkType), t));
        RuleFor(x => x.LinkedEntityId).NotEqual(Guid.Empty);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
