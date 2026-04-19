using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class CreatePerformanceGoalRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string TitleAr { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Weight { get; set; }

    public string? TargetValue { get; set; }

    public PerformanceGoalStatus Status { get; set; } = PerformanceGoalStatus.Draft;
}
