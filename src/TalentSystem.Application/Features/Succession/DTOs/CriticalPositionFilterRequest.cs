using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class CriticalPositionFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? PositionId { get; set; }

    public bool? ActiveOnly { get; set; }
}
