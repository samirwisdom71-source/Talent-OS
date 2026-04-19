namespace TalentSystem.Application.Features.Employees.DTOs;

public sealed class EmployeeDto
{
    public Guid Id { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string FullNameEn { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid OrganizationUnitId { get; set; }

    public Guid PositionId { get; set; }
}
