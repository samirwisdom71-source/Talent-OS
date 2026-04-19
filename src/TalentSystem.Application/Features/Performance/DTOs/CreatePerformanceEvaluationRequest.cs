using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class CreatePerformanceEvaluationRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public decimal OverallScore { get; set; }

    public string? ManagerComments { get; set; }

    public string? EmployeeComments { get; set; }

    public PerformanceEvaluationStatus Status { get; set; } = PerformanceEvaluationStatus.Draft;
}
