using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Potential.DTOs;

public sealed class CreatePotentialAssessmentRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public Guid AssessedByEmployeeId { get; set; }

    public decimal AgilityScore { get; set; }

    public decimal LeadershipScore { get; set; }

    public decimal GrowthScore { get; set; }

    public decimal MobilityScore { get; set; }

    public string? Comments { get; set; }

    public PotentialAssessmentStatus Status { get; set; } = PotentialAssessmentStatus.Draft;

    public IReadOnlyList<PotentialAssessmentFactorItemDto> Factors { get; set; } = Array.Empty<PotentialAssessmentFactorItemDto>();
}
