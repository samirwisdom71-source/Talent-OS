namespace TalentSystem.Application.Features.JobArchitecture.DTOs;

public sealed class JobGradeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }
}
