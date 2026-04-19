using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class UpdatePerformanceEvaluationRequest
{
    public decimal OverallScore { get; set; }

    public string? ManagerComments { get; set; }

    public string? EmployeeComments { get; set; }

    public PerformanceEvaluationStatus Status { get; set; }
}
