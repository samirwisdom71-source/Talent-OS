using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.DTOs;

/// <summary>استعلام قائمة مختصرة لخطط التعاقب (معرّف الخطة + اسم الخطة).</summary>
public sealed class SuccessionPlanLookupRequest
{
    public string? Search { get; set; }

    public int Take { get; set; } = PaginationConstants.MaxPageSize;

    public Guid? CriticalPositionId { get; set; }

    public SuccessionPlanStatus? Status { get; set; }
}
