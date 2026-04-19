namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class SuccessionCoverageSnapshotDto
{
    public Guid Id { get; set; }

    public Guid SuccessionPlanId { get; set; }

    public int TotalCandidates { get; set; }

    public bool HasReadyNow { get; set; }

    public bool HasPrimarySuccessor { get; set; }

    public decimal CoverageScore { get; set; }

    public DateTime CalculatedOnUtc { get; set; }
}
