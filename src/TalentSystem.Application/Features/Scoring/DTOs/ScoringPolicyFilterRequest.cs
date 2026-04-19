using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class ScoringPolicyFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
