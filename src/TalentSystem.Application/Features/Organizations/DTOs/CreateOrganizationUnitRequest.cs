namespace TalentSystem.Application.Features.Organizations.DTOs;

public sealed class CreateOrganizationUnitRequest
{
    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}
