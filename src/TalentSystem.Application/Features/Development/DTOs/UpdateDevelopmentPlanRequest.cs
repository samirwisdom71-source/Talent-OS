using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class UpdateDevelopmentPlanRequest
{
    public string PlanTitle { get; set; } = string.Empty;

    public DevelopmentPlanSourceType SourceType { get; set; }

    public DateTime? TargetCompletionDate { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// When non-null, replaces all existing links for the plan with this set (soft-deletes prior links).
    /// </summary>
    public IReadOnlyList<DevelopmentPlanLinkInputDto>? Links { get; set; }
}
