using TalentSystem.Application.Features.Intelligence.Interfaces;
using TalentSystem.Application.Features.Intelligence.Models;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Application.Features.Intelligence.Services;

/// <summary>
/// Deterministic, explainable rules. Replaceable later with <see cref="InsightSource.FutureMl"/> providers.
/// </summary>
public sealed class RulesBasedIntelligenceProvider : IIntelligenceProvider
{
    private const decimal ScoreHigh = 70m;

    private const decimal ScoreLow = 45m;

    public IReadOnlyList<TalentInsight> BuildInsights(EmployeeIntelligenceContext context)
    {
        if (!HasAnySignal(context))
        {
            return Array.Empty<TalentInsight>();
        }

        var utc = DateTime.UtcNow;
        var list = new List<TalentInsight>();

        if (IsStarTalent(context))
        {
            list.Add(CreateInsight(
                context,
                InsightType.SuccessorReadiness,
                InsightSeverity.High,
                88,
                "Successor readiness signal",
                "Performance and potential signals align with top-talent succession profiles for this cycle.",
                utc));

            list.Add(CreateInsight(
                context,
                InsightType.PotentialSignal,
                InsightSeverity.Medium,
                80,
                "Strong potential trajectory",
                "Potential indicators reinforce leadership pipeline consideration alongside performance outcomes.",
                utc));
        }

        if (IsDevelopmentPriority(context) && !IsStarTalent(context))
        {
            list.Add(CreateInsight(
                context,
                InsightType.DevelopmentPriority,
                InsightSeverity.Medium,
                72,
                "Development priority",
                "Elevated potential relative to current performance suggests focused development intervention.",
                utc));
        }

        if (IsDiamondProfile(context))
        {
            list.Add(CreateInsight(
                context,
                InsightType.ClassificationAttention,
                InsightSeverity.Medium,
                78,
                "Classification attention",
                "Low performance with high potential requires calibration of expectations, support, and potential assessment.",
                utc));
        }

        if (IsTalentRisk(context))
        {
            list.Add(CreateInsight(
                context,
                InsightType.TalentRisk,
                InsightSeverity.High,
                75,
                "Talent risk signal",
                "Combined performance and potential indicators sit in a risk band that merits structured people review.",
                utc));

            list.Add(CreateInsight(
                context,
                InsightType.PerformanceConcern,
                InsightSeverity.Medium,
                68,
                "Performance concern",
                "Performance outcomes for the cycle are weak relative to organizational expectations.",
                utc));
        }

        return DedupeInsightsByType(list);
    }

    public IReadOnlyList<TalentRecommendation> BuildRecommendations(EmployeeIntelligenceContext context)
    {
        if (!HasAnySignal(context))
        {
            return Array.Empty<TalentRecommendation>();
        }

        var utc = DateTime.UtcNow;
        var list = new List<TalentRecommendation>();

        if (IsStarTalent(context))
        {
            if (context.IsPrimarySuccessor)
            {
                list.Add(CreateRecommendation(
                    context,
                    RecommendationType.LeadershipAcceleration,
                    RecommendationPriority.High,
                    86,
                    "Accelerate leadership readiness",
                    "Top-box signals with primary succession placement warrant accelerated exposure and governance review.",
                    "Schedule executive talent review and expand high-visibility assignments in line with succession policy.",
                    utc));
            }
            else
            {
                list.Add(CreateRecommendation(
                    context,
                    RecommendationType.ConsiderForSuccession,
                    RecommendationPriority.High,
                    84,
                    "Consider for succession coverage",
                    "Strong performance and potential alignment supports formal succession evaluation for critical roles.",
                    "Add or validate succession candidacy with calibrated readiness evidence for target critical positions.",
                    utc));
            }

            list.Add(CreateRecommendation(
                context,
                RecommendationType.ReviewSuccessionReadiness,
                RecommendationPriority.Medium,
                70,
                "Review succession readiness evidence",
                "Ensure readiness narrative, risk factors, and mobility constraints are documented for committee review.",
                "Update succession readiness notes and validate time-to-readiness assumptions with the employee's manager.",
                utc));
        }

        if (IsDevelopmentPriority(context) && !IsStarTalent(context))
        {
            var recType = context.HasActiveDevelopmentPlan
                ? RecommendationType.ReviewPerformance
                : RecommendationType.AssignDevelopmentPlan;

            list.Add(CreateRecommendation(
                context,
                recType,
                RecommendationPriority.High,
                74,
                recType == RecommendationType.AssignDevelopmentPlan
                    ? "Assign a development plan"
                    : "Review performance drivers",
                "Potential exceeds realized performance; structured performance or development support is recommended.",
                recType == RecommendationType.AssignDevelopmentPlan
                    ? "Create or activate a development plan anchored to the lowest-scoring performance themes for this cycle."
                    : "Facilitate a performance diagnosis session and align goals before adjusting potential ratings.",
                utc));
        }

        if (IsDiamondProfile(context))
        {
            list.Add(CreateRecommendation(
                context,
                RecommendationType.ReassessPotential,
                RecommendationPriority.Medium,
                76,
                "Reassess potential calibration",
                "Classic high-potential / low-performance cell often reflects contextual barriers or calibration drift.",
                "Revisit potential assessment inputs with HRBP and validate environmental constraints before final labels.",
                utc));
        }

        if (IsMarketplaceFit(context))
        {
            list.Add(CreateRecommendation(
                context,
                RecommendationType.ConsiderMarketplaceOpportunity,
                RecommendationPriority.Medium,
                71,
                "Consider internal marketplace mobility",
                "Strong talent signals without primary succession designation may benefit from visible internal opportunities.",
                "Review open internal roles with high match potential and discuss interest during the next talent conversation.",
                utc));
        }

        if (IsTalentRisk(context))
        {
            list.Add(CreateRecommendation(
                context,
                RecommendationType.ReviewPerformance,
                RecommendationPriority.High,
                73,
                "Structured performance review",
                "Weak performance and potential combination requires documented expectations and support plan.",
                "Initiate a formal performance improvement discussion with HR guidance and clear milestone checkpoints.",
                utc));
        }

        return DedupeRecommendationsByType(list);
    }

    private static bool HasAnySignal(EmployeeIntelligenceContext context) =>
        context.HasClassification || context.PerformanceScore is not null || context.PotentialScore is not null;

    private static bool IsPerformanceHigh(EmployeeIntelligenceContext c) =>
        c.HasClassification && c.PerformanceBand is PerformanceBand.High
            || !c.HasClassification && c.PerformanceScore is >= ScoreHigh;

    private static bool IsPerformanceLow(EmployeeIntelligenceContext c) =>
        c.HasClassification && c.PerformanceBand is PerformanceBand.Low
            || !c.HasClassification && c.PerformanceScore is < ScoreLow;

    private static bool IsPotentialHigh(EmployeeIntelligenceContext c) =>
        c.HasClassification && c.PotentialBand is PotentialBand.High
            || !c.HasClassification && c.PotentialScore is >= ScoreHigh;

    private static bool IsPotentialMediumOrHigh(EmployeeIntelligenceContext c) =>
        c.HasClassification
            ? c.PotentialBand is PotentialBand.Medium || c.PotentialBand is PotentialBand.High
            : c.PotentialScore is >= ScoreLow;

    private static bool IsPotentialLow(EmployeeIntelligenceContext c) =>
        c.HasClassification && c.PotentialBand is PotentialBand.Low
            || !c.HasClassification && c.PotentialScore is < ScoreLow;

    private static bool IsStarTalent(EmployeeIntelligenceContext c) =>
        c is { IsHighPotential: true, IsHighPerformer: true }
            || c.NineBoxCode == NineBoxCode.Box9
            || (!c.HasClassification && IsPerformanceHigh(c) && IsPotentialHigh(c));

    private static bool IsDevelopmentPriority(EmployeeIntelligenceContext c) =>
        IsPotentialMediumOrHigh(c) && IsPerformanceLow(c);

    private static bool IsDiamondProfile(EmployeeIntelligenceContext c) =>
        IsPerformanceLow(c) && IsPotentialHigh(c);

    private static bool IsTalentRisk(EmployeeIntelligenceContext c) =>
        IsPerformanceLow(c) && IsPotentialLow(c);

    private static bool IsMarketplaceFit(EmployeeIntelligenceContext c) =>
        !c.IsPrimarySuccessor
            && IsPotentialHigh(c)
            && !IsPerformanceLow(c)
            && (c.IsHighPerformer || c.FinalScore is >= ScoreHigh || IsPerformanceHigh(c));

    private static TalentInsight CreateInsight(
        EmployeeIntelligenceContext context,
        InsightType type,
        InsightSeverity severity,
        byte confidence,
        string title,
        string summary,
        DateTime generatedOnUtc) =>
        new()
        {
            EmployeeId = context.EmployeeId,
            PerformanceCycleId = context.PerformanceCycleId,
            InsightType = type,
            Severity = severity,
            Source = InsightSource.RulesEngine,
            Title = title,
            Summary = summary,
            ConfidenceScore = confidence,
            Status = TalentInsightStatus.Active,
            GeneratedOnUtc = generatedOnUtc
        };

    private static TalentRecommendation CreateRecommendation(
        EmployeeIntelligenceContext context,
        RecommendationType type,
        RecommendationPriority priority,
        byte confidence,
        string title,
        string description,
        string action,
        DateTime generatedOnUtc) =>
        new()
        {
            EmployeeId = context.EmployeeId,
            PerformanceCycleId = context.PerformanceCycleId,
            RecommendationType = type,
            Priority = priority,
            Source = RecommendationSource.RulesEngine,
            Title = title,
            Description = description,
            RecommendedAction = action,
            ConfidenceScore = confidence,
            Status = TalentRecommendationStatus.Active,
            GeneratedOnUtc = generatedOnUtc
        };

    private static List<TalentInsight> DedupeInsightsByType(List<TalentInsight> items)
    {
        var map = new Dictionary<InsightType, TalentInsight>();
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.InsightType, out var existing) || existing.Severity < item.Severity)
            {
                map[item.InsightType] = item;
            }
        }

        return map.Values.ToList();
    }

    private static List<TalentRecommendation> DedupeRecommendationsByType(List<TalentRecommendation> items)
    {
        var map = new Dictionary<RecommendationType, TalentRecommendation>();
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.RecommendationType, out var existing) || existing.Priority < item.Priority)
            {
                map[item.RecommendationType] = item;
            }
        }

        return map.Values.ToList();
    }
}
