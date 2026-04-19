using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class CriticalPositionDto
{
    public Guid Id { get; set; }

    public Guid PositionId { get; set; }

    public CriticalityLevel CriticalityLevel { get; set; }

    public SuccessionRiskLevel RiskLevel { get; set; }

    public string? Notes { get; set; }

    public RecordStatus RecordStatus { get; set; }
}
