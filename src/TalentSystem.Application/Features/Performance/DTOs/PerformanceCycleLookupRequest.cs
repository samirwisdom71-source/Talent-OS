using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Constants;

namespace TalentSystem.Application.Features.Performance.DTOs;

/// <summary>
/// طلب قائمة مختصرة (معرّف + اسم) لدورات الأداء للقوائم المنسدلة.
/// </summary>
public sealed class PerformanceCycleLookupRequest
{
    public string? Search { get; set; }

    public PerformanceCycleStatus? Status { get; set; }

    /// <summary>أقصى عدد للسجلات (يُقيَّد تلقائياً حتى <see cref="PaginationConstants.MaxPageSize"/>).</summary>
    public int Take { get; set; } = PaginationConstants.MaxPageSize;

    /// <summary>لغة عرض الحقل <c>Name</c>: <c>ar</c> (افتراضي) أو <c>en</c>.</summary>
    public string Lang { get; set; } = "ar";
}
