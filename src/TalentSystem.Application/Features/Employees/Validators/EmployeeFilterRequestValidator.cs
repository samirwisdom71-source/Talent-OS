using FluentValidation;
using TalentSystem.Application.Features.Employees.DTOs;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Employees.Validators;

public sealed class EmployeeFilterRequestValidator : AbstractValidator<EmployeeFilterRequest>
{
    public EmployeeFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(PaginationConstants.MaxPageSize);

        RuleFor(x => x.Search)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.Search));
    }
}
