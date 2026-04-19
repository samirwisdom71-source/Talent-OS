using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanLinkDto
{
    public Guid Id { get; set; }

    public Guid DevelopmentPlanId { get; set; }

    public DevelopmentPlanLinkType LinkType { get; set; }

    public Guid LinkedEntityId { get; set; }

    public string? Notes { get; set; }
}
