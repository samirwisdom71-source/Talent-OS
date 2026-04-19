using TalentSystem.Application.Features.Intelligence.Models;
using TalentSystem.Domain.Intelligence;

namespace TalentSystem.Application.Features.Intelligence.Interfaces;

/// <summary>
/// Pluggable intelligence engine. Current implementation is deterministic rules-based.
/// </summary>
public interface IIntelligenceProvider
{
    IReadOnlyList<TalentInsight> BuildInsights(EmployeeIntelligenceContext context);

    IReadOnlyList<TalentRecommendation> BuildRecommendations(EmployeeIntelligenceContext context);
}
