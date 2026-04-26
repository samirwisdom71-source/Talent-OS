using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/analytics/performance")]
public sealed class PerformanceAnalyticsController : ControllerBase
{
    private readonly IPerformanceAnalyticsService _analytics;

    public PerformanceAnalyticsController(IPerformanceAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<PerformanceAnalyticsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var dateRange = AnalyticsDateRangeFilter.FromOptional(fromUtc, toUtc);
        var result = await _analytics.GetSummaryAsync(dateRange, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("impact")]
    [ProducesResponseType(typeof(ApiResponse<PerformanceImpactSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PerformanceImpactSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetImpact(
        [FromQuery] PerformanceImpactFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _analytics.GetImpactAsync(request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
