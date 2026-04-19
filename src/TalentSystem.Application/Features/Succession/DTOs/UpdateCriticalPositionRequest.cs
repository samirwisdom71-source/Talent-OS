using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class UpdateCriticalPositionRequest
{
    public CriticalityLevel CriticalityLevel { get; set; }

    public SuccessionRiskLevel RiskLevel { get; set; }

    public string? Notes { get; set; }
}
