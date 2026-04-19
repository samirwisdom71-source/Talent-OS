using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;

namespace TalentSystem.Domain.Talent;

public sealed class TalentProfile : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public DateTime? HireDate { get; set; }

    public string? Summary { get; set; }

    public Employee Employee { get; set; } = null!;
}
