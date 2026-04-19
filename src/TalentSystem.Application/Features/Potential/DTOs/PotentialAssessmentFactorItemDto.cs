namespace TalentSystem.Application.Features.Potential.DTOs;

public sealed class PotentialAssessmentFactorItemDto
{
    public string FactorName { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public decimal Weight { get; set; }

    public string? Notes { get; set; }
}
