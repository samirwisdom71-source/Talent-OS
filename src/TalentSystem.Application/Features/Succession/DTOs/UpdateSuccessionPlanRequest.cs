namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class UpdateSuccessionPlanRequest
{
    public string PlanName { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
