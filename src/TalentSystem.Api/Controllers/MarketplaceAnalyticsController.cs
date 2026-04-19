using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Analytics.DTOs;
using TalentSystem.Application.Features.Analytics.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/analytics/marketplace")]
public sealed class MarketplaceAnalyticsController : ControllerBase
{
    private readonly IMarketplaceAnalyticsService _analytics;

    public MarketplaceAnalyticsController(IMarketplaceAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<MarketplaceAnalyticsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _analytics.GetSummaryAsync(cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
