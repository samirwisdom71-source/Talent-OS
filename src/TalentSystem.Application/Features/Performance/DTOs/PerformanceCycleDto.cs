using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class PerformanceCycleDto
{
    public Guid Id { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public PerformanceCycleStatus Status { get; set; }

    public string? Description { get; set; }
}
