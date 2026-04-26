using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class SuggestDevelopmentPlanRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public DevelopmentPlanSourceType SourceType { get; set; }
}

/// <summary>مسودة مقترحة من النظام — يمكن إرسالها لاحقاً مع إنشاء الخطة (StructuredItems).</summary>
public sealed class DevelopmentPlanSuggestionDto
{
    public string PlanTitle { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public IReadOnlyList<DevelopmentPlanLinkInputDto> Links { get; set; } = Array.Empty<DevelopmentPlanLinkInputDto>();

    public IReadOnlyList<DevelopmentPlanStructuredItemInputDto> Items { get; set; } =
        Array.Empty<DevelopmentPlanStructuredItemInputDto>();
}

public sealed class DevelopmentPlanStructuredItemInputDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DevelopmentItemType ItemType { get; set; }

    public Guid? RelatedCompetencyId { get; set; }

    public DateTime? TargetDate { get; set; }

    public IReadOnlyList<CreateDevelopmentPlanItemPathRequest>? Paths { get; set; }
}
