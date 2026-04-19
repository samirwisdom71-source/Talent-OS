using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }
}
