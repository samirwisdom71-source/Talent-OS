using TalentSystem.Application.Features.Intelligence.Models;
using TalentSystem.Application.Features.Intelligence.Services;
using TalentSystem.Domain.Enums;
using TalentSystem.Domain.Intelligence;

namespace TalentSystem.UnitTests;

public sealed class RulesBasedIntelligenceProviderTests
{
    private readonly RulesBasedIntelligenceProvider _provider = new();

    [Fact]
    public void BuildInsights_StarProfile_ReturnsSuccessorReadiness()
    {
        var ctx = new EmployeeIntelligenceContext
        {
            EmployeeId = Guid.NewGuid(),
            PerformanceCycleId = Guid.NewGuid(),
            HasClassification = true,
            PerformanceBand = PerformanceBand.High,
            PotentialBand = PotentialBand.High,
            NineBoxCode = NineBoxCode.Box9,
            IsHighPotential = true,
            IsHighPerformer = true,
            PerformanceScore = 90m,
            PotentialScore = 88m,
            FinalScore = 89m,
            IsPrimarySuccessor = false,
            HasActiveDevelopmentPlan = false
        };

        var insights = _provider.BuildInsights(ctx);

        Assert.Contains(insights, i => i.InsightType == InsightType.SuccessorReadiness);
        Assert.All(insights, i => Assert.Equal(InsightSource.RulesEngine, i.Source));
    }

    [Fact]
    public void BuildRecommendations_LowDualBand_ReturnsReviewPerformance()
    {
        var ctx = new EmployeeIntelligenceContext
        {
            EmployeeId = Guid.NewGuid(),
            PerformanceCycleId = Guid.NewGuid(),
            HasClassification = true,
            PerformanceBand = PerformanceBand.Low,
            PotentialBand = PotentialBand.Low,
            NineBoxCode = NineBoxCode.Box1,
            IsHighPotential = false,
            IsHighPerformer = false,
            PerformanceScore = 30m,
            PotentialScore = 32m,
            FinalScore = 31m,
            IsPrimarySuccessor = false,
            HasActiveDevelopmentPlan = false
        };

        var recs = _provider.BuildRecommendations(ctx);

        Assert.Contains(recs, r => r.RecommendationType == RecommendationType.ReviewPerformance);
    }

    [Fact]
    public void BuildInsights_NoSignals_ReturnsEmpty()
    {
        var ctx = new EmployeeIntelligenceContext
        {
            EmployeeId = Guid.NewGuid(),
            PerformanceCycleId = Guid.NewGuid(),
            HasClassification = false,
            PerformanceScore = null,
            PotentialScore = null,
            FinalScore = null,
            IsPrimarySuccessor = false,
            HasActiveDevelopmentPlan = false
        };

        Assert.Empty(_provider.BuildInsights(ctx));
        Assert.Empty(_provider.BuildRecommendations(ctx));
    }
}
