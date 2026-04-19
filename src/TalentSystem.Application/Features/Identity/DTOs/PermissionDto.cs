namespace TalentSystem.Application.Features.Identity.DTOs;

public sealed class PermissionDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Module { get; init; } = string.Empty;
}
