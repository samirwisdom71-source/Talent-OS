namespace TalentSystem.Application.Features.Organizations.DTOs;

public sealed class OrganizationUnitFilterRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public Guid? ParentId { get; set; }
}
