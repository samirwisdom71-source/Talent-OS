using TalentSystem.Domain.Common;

namespace TalentSystem.Domain.Classification;

public sealed class ClassificationRuleSet : AuditableDomainEntity
{
    public string Name { get; set; } = null!;

    public string Version { get; set; } = null!;

    public decimal LowThreshold { get; set; }

    public decimal HighThreshold { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public string? Notes { get; set; }
}
