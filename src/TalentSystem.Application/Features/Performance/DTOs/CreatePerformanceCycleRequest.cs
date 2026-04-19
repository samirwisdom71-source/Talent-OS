namespace TalentSystem.Application.Features.Performance.DTOs;

public sealed class CreatePerformanceCycleRequest
{
    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Description { get; set; }
}
