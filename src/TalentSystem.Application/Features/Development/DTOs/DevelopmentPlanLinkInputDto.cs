using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanLinkInputDto
{
    public DevelopmentPlanLinkType LinkType { get; set; }

    public Guid LinkedEntityId { get; set; }

    public string? Notes { get; set; }
}
