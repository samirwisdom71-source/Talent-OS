using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class SuccessionPlanDto
{
    public Guid Id { get; set; }

    public Guid CriticalPositionId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public SuccessionPlanStatus Status { get; set; }

    public string? Notes { get; set; }

    public SuccessionCoverageSnapshotDto? CoverageSnapshot { get; set; }
}
