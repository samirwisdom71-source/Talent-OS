using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Potential.DTOs;

public sealed class PotentialAssessmentFilterRequest
{
    public int Page { get; set; } = PaginationConstants.DefaultPage;

    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;

    public Guid? EmployeeId { get; set; }

    public Guid? PerformanceCycleId { get; set; }

    public PotentialLevel? PotentialLevel { get; set; }

    public PotentialAssessmentStatus? Status { get; set; }
}
