using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class ClassificationRuleSetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public decimal LowThreshold { get; set; }

    public decimal HighThreshold { get; set; }

    public DateTime EffectiveFromUtc { get; set; }

    public string? Notes { get; set; }

    public RecordStatus RecordStatus { get; set; }
}
