using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class ClassificationRuleSetFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public string? Search { get; set; }
}
