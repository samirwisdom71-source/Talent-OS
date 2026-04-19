using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Identity;
using TalentSystem.Application.Features.Intelligence.DTOs;
using TalentSystem.Application.Features.Intelligence.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/intelligence")]
[Authorize]
public sealed class IntelligenceController : ControllerBase
{
    private readonly IIntelligenceService _intelligence;

    public IntelligenceController(IIntelligenceService intelligence)
    {
        _intelligence = intelligence;
    }

    /// <summary>
    /// Runs rules-based intelligence for one employee. Use <see cref="GenerateEmployeeIntelligenceRequest.Target"/> to choose insights, recommendations, or both.
    /// </summary>
    [HttpPost("generate/employee")]
    [Authorize(Policy = PermissionCodes.IntelligenceGenerate)]
    public async Task<IActionResult> GenerateForEmployee(
        [FromBody] GenerateEmployeeIntelligenceRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = request.Target switch
        {
            EmployeeIntelligenceGenerationTarget.Insights =>
                await _intelligence.GenerateInsightsForEmployeeAsync(request, cancellationToken)
                    .ConfigureAwait(false),
            EmployeeIntelligenceGenerationTarget.Recommendations =>
                await _intelligence.GenerateRecommendationsForEmployeeAsync(request, cancellationToken)
                    .ConfigureAwait(false),
            _ => await _intelligence.GenerateAllForEmployeeAsync(request, cancellationToken).ConfigureAwait(false)
        };

        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("generate/cycle")]
    [Authorize(Policy = PermissionCodes.IntelligenceGenerate)]
    public async Task<IActionResult> GenerateForCycle(
        [FromBody] GenerateCycleIntelligenceRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.GenerateForPerformanceCycleAsync(request, cancellationToken)
            .ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("insights")]
    [Authorize(Policy = PermissionCodes.IntelligenceView)]
    public async Task<IActionResult> GetInsights(
        [FromQuery] TalentInsightFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.GetPagedInsightsAsync(request, cancellationToken).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<TalentInsightDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("insights/{id:guid}")]
    [Authorize(Policy = PermissionCodes.IntelligenceView)]
    public async Task<IActionResult> GetInsightById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.GetInsightByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("recommendations")]
    [Authorize(Policy = PermissionCodes.IntelligenceView)]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] TalentRecommendationFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.GetPagedRecommendationsAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<TalentRecommendationDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("recommendations/{id:guid}")]
    [Authorize(Policy = PermissionCodes.IntelligenceView)]
    public async Task<IActionResult> GetRecommendationById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.GetRecommendationByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("insights/{id:guid}/dismiss")]
    [Authorize(Policy = PermissionCodes.IntelligenceManage)]
    public async Task<IActionResult> DismissInsight(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.DismissInsightAsync(id, cancellationToken).ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("recommendations/{id:guid}/dismiss")]
    [Authorize(Policy = PermissionCodes.IntelligenceManage)]
    public async Task<IActionResult> DismissRecommendation(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.DismissRecommendationAsync(id, cancellationToken).ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("recommendations/{id:guid}/accept")]
    [Authorize(Policy = PermissionCodes.IntelligenceManage)]
    public async Task<IActionResult> AcceptRecommendation(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _intelligence.AcceptRecommendationAsync(id, cancellationToken).ConfigureAwait(false);
        return result.ToApiActionResult(this, traceId);
    }
}
