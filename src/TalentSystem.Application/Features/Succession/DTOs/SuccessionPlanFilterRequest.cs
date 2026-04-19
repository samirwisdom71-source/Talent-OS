using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class SuccessionPlanFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? CriticalPositionId { get; set; }

    public Guid? PerformanceCycleId { get; set; }
}
