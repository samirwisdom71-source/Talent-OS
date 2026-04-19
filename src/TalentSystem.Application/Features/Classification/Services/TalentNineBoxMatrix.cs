using TalentSystem.Domain.Enums;

namespace TalentSystem.Application.Features.Classification.Services;

/// <summary>
/// Central 9-box mapping: performance (horizontal: Low→High) × potential (vertical: Low→High).
/// </summary>
internal static class TalentNineBoxMatrix
{
    public static (NineBoxCode Code, string CategoryName) Resolve(
        PerformanceBand performance,
        PotentialBand potential) =>
        (performance, potential) switch
        {
            (PerformanceBand.Low, PotentialBand.Low) => (NineBoxCode.Box1, "At Risk Talent"),
            (PerformanceBand.Medium, PotentialBand.Low) => (NineBoxCode.Box2, "Solid Professional"),
            (PerformanceBand.High, PotentialBand.Low) => (NineBoxCode.Box3, "Efficient Professional"),
            (PerformanceBand.Low, PotentialBand.Medium) => (NineBoxCode.Box4, "Emerging Contributor"),
            (PerformanceBand.Medium, PotentialBand.Medium) => (NineBoxCode.Box5, "Core Contributor"),
            (PerformanceBand.High, PotentialBand.Medium) => (NineBoxCode.Box6, "High Performer"),
            (PerformanceBand.Low, PotentialBand.High) => (NineBoxCode.Box7, "Growth Talent"),
            (PerformanceBand.Medium, PotentialBand.High) => (NineBoxCode.Box8, "Future Leader"),
            (PerformanceBand.High, PotentialBand.High) => (NineBoxCode.Box9, "Strategic Leader"),
            _ => throw new InvalidOperationException("Invalid performance and potential band combination.")
        };

    public static PerformanceBand ResolvePerformanceBand(decimal score, decimal lowThreshold, decimal highThreshold)
    {
        if (score < lowThreshold)
        {
            return PerformanceBand.Low;
        }

        if (score < highThreshold)
        {
            return PerformanceBand.Medium;
        }

        return PerformanceBand.High;
    }

    public static PotentialBand ResolvePotentialBand(decimal score, decimal lowThreshold, decimal highThreshold)
    {
        if (score < lowThreshold)
        {
            return PotentialBand.Low;
        }

        if (score < highThreshold)
        {
            return PotentialBand.Medium;
        }

        return PotentialBand.High;
    }
}
