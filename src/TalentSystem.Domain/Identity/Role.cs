using TalentSystem.Shared.Abstractions;

namespace TalentSystem.Domain.Identity;

public sealed class Role : AuditableEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
