namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class UpdateClassificationRuleSetRequest
{
    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public decimal LowThreshold { get; set; }

    public decimal HighThreshold { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public string? Notes { get; set; }
}
