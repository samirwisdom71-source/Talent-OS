using FluentValidation;
using TalentSystem.Application.Features.Employees.DTOs;

namespace TalentSystem.Application.Features.Employees.Validators;

public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.FullNameAr)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.FullNameEn)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320)
            .EmailAddress();

        RuleFor(x => x.OrganizationUnitId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.PositionId)
            .NotEqual(Guid.Empty);
    }
}
