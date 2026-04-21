namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class JobGradeFilterRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public int? Level { get; set; }
}
