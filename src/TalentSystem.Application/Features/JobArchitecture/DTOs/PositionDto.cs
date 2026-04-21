namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class PositionDto
{
    public Guid Id { get; set; }

    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public Guid OrganizationUnitId { get; set; }

    public string? OrganizationUnitNameAr { get; set; }

    public string? OrganizationUnitNameEn { get; set; }

    public Guid JobGradeId { get; set; }

    public string? JobGradeName { get; set; }

    public int JobGradeLevel { get; set; }
}
