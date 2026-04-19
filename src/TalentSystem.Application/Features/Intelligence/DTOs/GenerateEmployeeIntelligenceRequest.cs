namespace TalentSystem.Application.Features.Intelligence.DTOs;

public sealed class GenerateEmployeeIntelligenceRequest
{
    public Guid EmployeeId { get; init; }

    public Guid PerformanceCycleId { get; init; }

    /// <summary>Defaults to <see cref="EmployeeIntelligenceGenerationTarget.All"/>.</summary>
    public EmployeeIntelligenceGenerationTarget Target { get; init; } = EmployeeIntelligenceGenerationTarget.All;
}
