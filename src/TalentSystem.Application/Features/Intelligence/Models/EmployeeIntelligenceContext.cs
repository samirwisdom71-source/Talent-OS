using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Intelligence.Models;

/// <summary>
/// Read-model snapshot passed to intelligence providers. Built only from persisted domain data.
/// </summary>
public sealed class EmployeeIntelligenceContext
{
    public Guid EmployeeId { get; init; }

    public Guid PerformanceCycleId { get; init; }

    public bool HasClassification { get; init; }

    public PerformanceBand? PerformanceBand { get; init; }

    public PotentialBand? PotentialBand { get; init; }

    public NineBoxCode? NineBoxCode { get; init; }

    public bool IsHighPotential { get; init; }

    public bool IsHighPerformer { get; init; }

    public decimal? PerformanceScore { get; init; }

    public decimal? PotentialScore { get; init; }

    public decimal? FinalScore { get; init; }

    public bool IsPrimarySuccessor { get; init; }

    public bool HasActiveDevelopmentPlan { get; init; }
}
