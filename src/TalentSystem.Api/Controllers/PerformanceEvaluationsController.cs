using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Application.Features.Performance.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/performance-evaluations")]
public sealed class PerformanceEvaluationsController : ControllerBase
{
    private readonly IPerformanceEvaluationService _service;

    public PerformanceEvaluationsController(IPerformanceEvaluationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePerformanceEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<PerformanceEvaluationDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePerformanceEvaluationRequest request,
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
        [FromQuery] PerformanceEvaluationFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<PerformanceEvaluationDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }
}
