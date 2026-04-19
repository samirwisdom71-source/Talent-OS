using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanItemFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid DevelopmentPlanId { get; set; }
}
