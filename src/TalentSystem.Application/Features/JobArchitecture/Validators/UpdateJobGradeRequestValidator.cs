using FluentValidation;
using TalentSystem.Application.Features.JobArchitecture.DTOs;

namespace TalentSystem.Application.Features.JobArchitecture.Validators;

public sealed class UpdateJobGradeRequestValidator : AbstractValidator<UpdateJobGradeRequest>
{
    public UpdateJobGradeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Level).GreaterThan(0);
    }
}
