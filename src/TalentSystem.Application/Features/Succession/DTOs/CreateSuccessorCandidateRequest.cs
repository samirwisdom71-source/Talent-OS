using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Succession.DTOs;

public sealed class CreateSuccessorCandidateRequest
{
    public Guid SuccessionPlanId { get; set; }

    public Guid EmployeeId { get; set; }

    public ReadinessLevel ReadinessLevel { get; set; }

    public int RankOrder { get; set; }

    public bool IsPrimarySuccessor { get; set; }

    public string? Notes { get; set; }
}
