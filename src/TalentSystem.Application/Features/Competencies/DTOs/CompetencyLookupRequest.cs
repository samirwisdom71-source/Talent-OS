using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.DTOs;

/// <summary>استعلام قائمة مختصرة للكفاءات (معرّف + اسم للعرض).</summary>
public sealed class CompetencyLookupRequest
{
    public string? Search { get; set; }

    public int Take { get; set; } = PaginationConstants.MaxPageSize;

    public string Lang { get; set; } = "ar";
}
