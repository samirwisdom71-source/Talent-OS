using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Development.DTOs;

public sealed class DevelopmentPlanItemPathHelperDto
{
    public Guid Id { get; set; }

    public Guid DevelopmentPlanItemPathId { get; set; }

    public PathHelperKind HelperKind { get; set; }

    public Guid HelperEntityId { get; set; }
}

public sealed class DevelopmentPlanItemPathDto
{
    public Guid Id { get; set; }

    public Guid DevelopmentPlanItemId { get; set; }

    public int SortOrder { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? PlannedStartUtc { get; set; }

    public DateTime? PlannedEndUtc { get; set; }

    public DevelopmentItemStatus Status { get; set; }

    /// <summary>الأثر المحقق عند إكمال المسار (نقاط ضمن مقياس 100 للخطة).</summary>
    public decimal? AchievedImpactValue { get; set; }

    public IReadOnlyList<DevelopmentPlanItemPathHelperDto> Helpers { get; set; } = Array.Empty<DevelopmentPlanItemPathHelperDto>();
}

public sealed class DevelopmentPlanItemPathHelperInputDto
{
    public PathHelperKind HelperKind { get; set; }

    public Guid HelperEntityId { get; set; }
}

public sealed class CreateDevelopmentPlanItemPathRequest
{
    public int SortOrder { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? PlannedStartUtc { get; set; }

    public DateTime? PlannedEndUtc { get; set; }

    public IReadOnlyList<DevelopmentPlanItemPathHelperInputDto>? Helpers { get; set; }
}

public sealed class UpdateDevelopmentPlanItemPathRequest
{
    public int SortOrder { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? PlannedStartUtc { get; set; }

    public DateTime? PlannedEndUtc { get; set; }

    public DevelopmentItemStatus Status { get; set; }

    public IReadOnlyList<DevelopmentPlanItemPathHelperInputDto>? Helpers { get; set; }
}
