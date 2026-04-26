using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanTitle { get; set; } = string.Empty;

    public DevelopmentPlanSourceType SourceType { get; set; }

    public bool IsSystemSuggested { get; set; }

    public DevelopmentPlanStatus Status { get; set; }

    public DateTime? TargetCompletionDate { get; set; }

    public string? Notes { get; set; }

    public Guid? ApprovedByEmployeeId { get; set; }

    public DateTime? ApprovedOnUtc { get; set; }

    public IReadOnlyList<DevelopmentPlanLinkDto>? Links { get; set; }

    public IReadOnlyList<DevelopmentPlanImpactSnapshotDto>? ImpactSnapshots { get; set; }
}
