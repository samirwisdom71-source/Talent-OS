namespace TalentSystem.Application.Features.Organizations.DTOs;

public sealed class OrganizationUnitDto
{
    public Guid Id { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public string? ParentNameAr { get; set; }

    public string? ParentNameEn { get; set; }
}
