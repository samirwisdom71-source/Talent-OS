using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Competencies.DTOs;

/// <summary>استعلام قائمة مختصرة لمستويات الكفاءة (معرّف + اسم للعرض).</summary>
public sealed class CompetencyLevelLookupRequest
{
    public string? Search { get; set; }

    public int Take { get; set; } = PaginationConstants.MaxPageSize;
}
