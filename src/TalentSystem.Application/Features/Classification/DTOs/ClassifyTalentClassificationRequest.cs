namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class ClassifyTalentClassificationRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string? Notes { get; set; }
}
