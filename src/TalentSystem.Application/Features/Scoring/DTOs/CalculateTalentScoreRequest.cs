namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class CalculateTalentScoreRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string? Notes { get; set; }
}
