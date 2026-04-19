using TalentSystem.Domain.Intelligence;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Intelligence.DTOs;

public sealed class TalentInsightFilterRequest
{
    public int Page { get; init; } = PaginationConstants.DefaultPage;

    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; init; }

    public Guid? PerformanceCycleId { get; init; }

    public TalentInsightStatus? Status { get; init; }
}
