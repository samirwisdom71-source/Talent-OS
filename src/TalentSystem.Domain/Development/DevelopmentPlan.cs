using TalentSystem.Domain.Common;
using TalentSystem.Domain.Employees;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Performance;

namespace TalentSystem.Domain.Development;

public sealed class DevelopmentPlan : AuditableDomainEntity
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanTitle { get; set; } = null!;

    public DevelopmentPlanSourceType SourceType { get; set; }

    public DevelopmentPlanStatus Status { get; set; } = DevelopmentPlanStatus.Draft;

    public DateTime? TargetCompletionDate { get; set; }

    public string? Notes { get; set; }

    public Guid? ApprovedByEmployeeId { get; set; }

    public DateTime? ApprovedOnUtc { get; set; }

    public Employee Employee { get; set; } = null!;

    public PerformanceCycle PerformanceCycle { get; set; } = null!;

    public Employee? ApprovedByEmployee { get; set; }

    public ICollection<DevelopmentPlanItem> Items { get; set; } = new List<DevelopmentPlanItem>();

    public ICollection<DevelopmentPlanLink> Links { get; set; } = new List<DevelopmentPlanLink>();
}
