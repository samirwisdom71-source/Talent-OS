using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class PerformanceCycleFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }

    public PerformanceCycleStatus? Status { get; set; }
}
