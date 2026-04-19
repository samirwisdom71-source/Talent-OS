using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class TalentClassificationDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public Guid TalentScoreId { get; set; }

    public PerformanceBand PerformanceBand { get; set; }

    public PotentialBand PotentialBand { get; set; }

    public NineBoxCode NineBoxCode { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsHighPotential { get; set; }

    public bool IsHighPerformer { get; set; }

    public DateTime ClassifiedOnUtc { get; set; }

    public string? Notes { get; set; }
}
