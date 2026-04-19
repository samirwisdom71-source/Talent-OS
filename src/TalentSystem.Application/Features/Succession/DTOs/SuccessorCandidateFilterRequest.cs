using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class SuccessorCandidateFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid SuccessionPlanId { get; set; }
}
