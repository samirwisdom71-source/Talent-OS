using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Employees.DTOs;

public sealed class EmployeeFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
