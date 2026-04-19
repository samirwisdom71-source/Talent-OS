using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/analytics/talent")]
public sealed class TalentAnalyticsController : ControllerBase
{
    private readonly ITalentAnalyticsService _analytics;

    public TalentAnalyticsController(ITalentAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("distribution")]
    [ProducesResponseType(typeof(ApiResponse<TalentDistributionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TalentDistributionSummaryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TalentDistributionSummaryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDistribution(
        [FromQuery] TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _analytics.GetDistributionAsync(filter, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("by-cycle")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TalentClassificationByCycleSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TalentClassificationByCycleSummaryDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TalentClassificationByCycleSummaryDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCycle(
        [FromQuery] TalentAnalyticsFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _analytics.GetByCycleAsync(filter, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
