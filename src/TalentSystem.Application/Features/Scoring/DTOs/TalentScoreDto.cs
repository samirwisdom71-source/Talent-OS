namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class TalentScoreDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public decimal PerformanceScore { get; set; }

    public decimal PotentialScore { get; set; }

    public decimal FinalScore { get; set; }

    public decimal PerformanceWeight { get; set; }

    public decimal PotentialWeight { get; set; }

    public string CalculationVersion { get; set; } = string.Empty;

    public DateTime CalculatedOnUtc { get; set; }

    public string? Notes { get; set; }
}
