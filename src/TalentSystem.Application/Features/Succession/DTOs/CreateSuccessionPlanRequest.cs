namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class CreateSuccessionPlanRequest
{
    public Guid CriticalPositionId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
