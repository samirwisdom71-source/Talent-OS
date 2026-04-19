namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class RecalculateTalentScoreRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string? Notes { get; set; }
}
