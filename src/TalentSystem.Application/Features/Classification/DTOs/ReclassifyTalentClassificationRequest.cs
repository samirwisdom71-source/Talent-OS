namespace TalentSystem.Application.Features.Classification.DTOs;

public sealed class ReclassifyTalentClassificationRequest
{
    public Guid EmployeeId { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string? Notes { get; set; }
}
