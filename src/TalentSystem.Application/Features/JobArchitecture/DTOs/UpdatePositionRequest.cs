namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class UpdatePositionRequest
{
    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public Guid OrganizationUnitId { get; set; }

    public Guid JobGradeId { get; set; }
}
