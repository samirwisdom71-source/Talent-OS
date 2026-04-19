using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.DTOs;

public sealed class JobCompetencyRequirementFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? PositionId { get; set; }

    public Guid? CompetencyId { get; set; }
}
