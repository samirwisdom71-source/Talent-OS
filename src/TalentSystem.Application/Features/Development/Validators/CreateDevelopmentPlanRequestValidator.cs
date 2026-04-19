using FluentValidation;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.Validators;

public sealed class CreateDevelopmentPlanRequestValidator : AbstractValidator<CreateDevelopmentPlanRequest>
{
    public CreateDevelopmentPlanRequestValidator(IValidator<DevelopmentPlanLinkInputDto> linkValidator)
    {
        RuleFor(x => x.EmployeeId).NotEqual(Guid.Empty);
        RuleFor(x => x.PerformanceCycleId).NotEqual(Guid.Empty);
        RuleFor(x => x.PlanTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SourceType).Must(s => Enum.IsDefined(typeof(DevelopmentPlanSourceType), s));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Notes));

        RuleForEach(x => x.Links).SetValidator(linkValidator)
            .When(x => x.Links is { Count: > 0 });
    }
}
