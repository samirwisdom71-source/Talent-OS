using TalentSystem.Domain.Employees;
using TalentSystem.Shared.Abstractions;

namespace TalentSystem.Domain.Identity;

public sealed class User : AuditableEntity
{
    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public Guid? EmployeeId { get; set; }

    public DateTime? LastLoginUtc { get; set; }
    
    public Employee? Employee { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
