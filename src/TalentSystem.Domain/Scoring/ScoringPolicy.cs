using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Scoring;

public sealed class ScoringPolicy : AuditableDomainEntity
{
    public string Name { get; set; } = null!;

    public string Version { get; set; } = null!;

    public decimal PerformanceWeight { get; set; }

    public decimal PotentialWeight { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public string? Notes { get; set; }
}
