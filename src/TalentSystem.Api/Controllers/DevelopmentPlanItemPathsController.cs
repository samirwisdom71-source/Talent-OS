using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/development-plan-items/{planItemId:guid}/paths")]
public sealed class DevelopmentPlanItemPathsController : ControllerBase
{
    private readonly IDevelopmentPlanItemPathService _paths;

    public DevelopmentPlanItemPathsController(IDevelopmentPlanItemPathService paths)
    {
        _paths = paths;
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid planItemId,
        [FromBody] CreateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _paths.AddAsync(planItemId, request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<DevelopmentPlanItemPathDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { planItemId, pathId = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpGet("{pathId:guid}")]
    public async Task<IActionResult> GetById(Guid planItemId, Guid pathId, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _paths.GetByIdAsync(pathId, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPut("{pathId:guid}")]
    public async Task<IActionResult> Update(
        Guid planItemId,
        Guid pathId,
        [FromBody] UpdateDevelopmentPlanItemPathRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _paths.UpdateAsync(pathId, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpDelete("{pathId:guid}")]
    public async Task<IActionResult> Remove(Guid planItemId, Guid pathId, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _paths.RemoveAsync(pathId, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{pathId:guid}/mark-completed")]
    public async Task<IActionResult> MarkCompleted(Guid planItemId, Guid pathId, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _paths.MarkCompletedAsync(pathId, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
