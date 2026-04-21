namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class UpdateJobGradeRequest
{
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }
}
