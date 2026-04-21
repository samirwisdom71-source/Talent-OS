namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class PositionFilterRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public Guid? JobGradeId { get; set; }
}
