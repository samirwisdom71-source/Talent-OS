using TalentSystem.Application.Features.Employees.DTOs;
using TalentSystem.Shared.Api;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Employees.Interfaces;

public interface IEmployeeService
{
    Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<EmployeeListItemDto>>> GetPagedAsync(
        EmployeeFilterRequest request,
        CancellationToken cancellationToken = default);
}
