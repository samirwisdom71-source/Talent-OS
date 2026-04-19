using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class TalentScoreFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public decimal? MinFinalScore { get; set; }

    public decimal? MaxFinalScore { get; set; }
}
