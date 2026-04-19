using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class CompetencyCategoryFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
