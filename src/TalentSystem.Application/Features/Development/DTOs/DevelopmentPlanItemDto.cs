using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanItemDto
{
    public Guid Id { get; set; }

    public Guid DevelopmentPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DevelopmentItemType ItemType { get; set; }

    public Guid? RelatedCompetencyId { get; set; }

    public DateTime? TargetDate { get; set; }

    public DevelopmentItemStatus Status { get; set; }

    public decimal ProgressPercentage { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<DevelopmentPlanItemPathDto>? Paths { get; set; }
}
