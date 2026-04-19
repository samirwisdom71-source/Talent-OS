using TalentSystem.Shared.Abstractions;

namespace TalentSystem.Domain.Identity;

public sealed class Permission : AuditableEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Module { get; set; } = null!;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
