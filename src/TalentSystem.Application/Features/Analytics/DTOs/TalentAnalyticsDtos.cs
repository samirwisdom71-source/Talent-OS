using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Analytics.DTOs;

public sealed class TalentAnalyticsFilterRequest
{
    public Guid? PerformanceCycleId { get; set; }

    public Guid? OrganizationUnitId { get; set; }
}

public sealed class TalentDistributionSummaryDto
{
    public IReadOnlyList<NineBoxDistributionItemDto> ByNineBox { get; init; } = Array.Empty<NineBoxDistributionItemDto>();

    public IReadOnlyList<EnumCountDto<PerformanceBand>> ByPerformanceBand { get; init; } =
        Array.Empty<EnumCountDto<PerformanceBand>>();

    public IReadOnlyList<EnumCountDto<PotentialBand>> ByPotentialBand { get; init; } =
        Array.Empty<EnumCountDto<PotentialBand>>();

    public IReadOnlyList<NamedCountDto> ByCategoryName { get; init; } = Array.Empty<NamedCountDto>();
}

public sealed class NineBoxDistributionItemDto
{
    public NineBoxCode NineBoxCode { get; init; }

    public int Count { get; init; }
}

public sealed class EnumCountDto<TEnum>
    where TEnum : struct, Enum
{
    public TEnum Value { get; init; }

    public int Count { get; init; }
}

public sealed class NamedCountDto
{
    public string Name { get; init; } = string.Empty;

    public int Count { get; init; }
}

public sealed class TalentClassificationByCycleSummaryDto
{
    public Guid PerformanceCycleId { get; init; }

    public string PerformanceCycleNameEn { get; init; } = string.Empty;

    public int ClassificationCount { get; init; }
}
