using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Organizations;

public sealed class OrganizationUnit : AuditableDomainEntity
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public OrganizationUnit? Parent { get; set; }

    public ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();
}
