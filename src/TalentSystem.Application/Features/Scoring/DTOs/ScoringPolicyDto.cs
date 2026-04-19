using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Scoring.DTOs;

public sealed class ScoringPolicyDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public decimal PerformanceWeight { get; set; }

    public decimal PotentialWeight { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public string? Notes { get; set; }

    public RecordStatus RecordStatus { get; set; }
}
