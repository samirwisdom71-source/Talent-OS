using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class CreateDevelopmentPlanRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string PlanTitle { get; set; } = string.Empty;

    public DevelopmentPlanSourceType SourceType { get; set; }

    public DateTime? TargetCompletionDate { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<DevelopmentPlanLinkInputDto>? Links { get; set; }
}
