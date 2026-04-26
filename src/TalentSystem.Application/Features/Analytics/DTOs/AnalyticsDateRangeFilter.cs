namespace TalentSystem.Application.Features.Analytics.DTOs;

/// <summary>Optional inclusive UTC range for analytics summaries. Both bounds required when either is set.</summary>
public sealed class AnalyticsDateRangeFilter
{
    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public static AnalyticsDateRangeFilter? FromOptional(DateTime? fromUtc, DateTime? toUtc)
    {
        if (!fromUtc.HasValue && !toUtc.HasValue)
        {
            return null;
        }

        return new AnalyticsDateRangeFilter { FromUtc = fromUtc, ToUtc = toUtc };
    }
}
