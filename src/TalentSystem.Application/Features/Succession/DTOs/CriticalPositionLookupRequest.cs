using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Succession.DTOs;

/// <summary>استعلام قائمة مختصرة للمناصب الحرجة (معرّف المنصب الحرج + اسم عرض من عنوان المنصب الوظيفي).</summary>
public sealed class CriticalPositionLookupRequest
{
    public string? Search { get; set; }

    public int Take { get; set; } = PaginationConstants.MaxPageSize;

    /// <summary>لغة عرض الاسم: ar أو en.</summary>
    public string Lang { get; set; } = "ar";

    /// <summary>عند true يُرجع السجلات النشطة فقط (RecordStatus = Active).</summary>
    public bool ActiveOnly { get; set; } = true;
}
