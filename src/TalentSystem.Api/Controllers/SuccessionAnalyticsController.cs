using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/analytics/succession")]
public sealed class SuccessionAnalyticsController : ControllerBase
{
    private readonly ISuccessionAnalyticsService _analytics;

    public SuccessionAnalyticsController(ISuccessionAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<SuccessionAnalyticsSummaryDto>), StatusCodes.Status200OK)]
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
}
