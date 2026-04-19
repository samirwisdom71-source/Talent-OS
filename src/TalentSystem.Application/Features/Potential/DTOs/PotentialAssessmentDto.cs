using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Potential.DTOs;

public sealed class PotentialAssessmentDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public Guid AssessedByEmployeeId { get; set; }

    public decimal AgilityScore { get; set; }

    public decimal LeadershipScore { get; set; }

    public decimal GrowthScore { get; set; }

    public decimal MobilityScore { get; set; }

    public decimal OverallPotentialScore { get; set; }

    public PotentialLevel PotentialLevel { get; set; }

    public string? Comments { get; set; }

    public PotentialAssessmentStatus Status { get; set; }

    public DateTime? AssessedOnUtc { get; set; }

    public IReadOnlyList<PotentialAssessmentFactorDto> Factors { get; set; } = Array.Empty<PotentialAssessmentFactorDto>();
}
