using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/development-plan-items")]
public sealed class DevelopmentPlanItemsController : ControllerBase
{
    private readonly IDevelopmentPlanItemService _service;

    public DevelopmentPlanItemsController(IDevelopmentPlanItemService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.AddAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<DevelopmentPlanItemDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDevelopmentPlanItemRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] DevelopmentPlanItemFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<DevelopmentPlanItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.RemoveAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/update-progress")]
    public async Task<IActionResult> UpdateProgress(
        Guid id,
        [FromBody] UpdateDevelopmentPlanItemProgressRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.UpdateProgressAsync(id, request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/mark-completed")]
    public async Task<IActionResult> MarkCompleted(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.MarkCompletedAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
