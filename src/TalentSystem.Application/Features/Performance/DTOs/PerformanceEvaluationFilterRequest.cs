using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class PerformanceEvaluationFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }
}
