using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanImpactSnapshotDto
{
    public Guid Id { get; set; }

    public Guid DevelopmentPlanId { get; set; }

    public DevelopmentImpactPhase Phase { get; set; }

    public DateTime RecordedOnUtc { get; set; }

    public string? SummaryNotes { get; set; }

    public decimal? MetricScore { get; set; }
}

public sealed class RecordDevelopmentPlanImpactRequest
{
    public DevelopmentImpactPhase Phase { get; set; }

    public DateTime? RecordedOnUtc { get; set; }

    public string? SummaryNotes { get; set; }

    public decimal? MetricScore { get; set; }
}
