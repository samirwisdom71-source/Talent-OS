using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Scoring.DTOs;
using TalentSystem.Application.Features.Scoring.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/talent-scores")]
public sealed class TalentScoresController : ControllerBase
{
    private readonly ITalentScoreService _service;

    public TalentScoresController(ITalentScoreService service)
    {
        _service = service;
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateTalentScoreRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.CalculateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<TalentScoreDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("recalculate")]
    public async Task<IActionResult> Recalculate(
        [FromBody] RecalculateTalentScoreRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.RecalculateAsync(request, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("by-employee-cycle")]
    public async Task<IActionResult> GetByEmployeeAndCycle(
        [FromQuery] Guid employeeId,
        [FromQuery] Guid performanceCycleId,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetByEmployeeAndCycleAsync(employeeId, performanceCycleId, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] TalentScoreFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<TalentScoreDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }
}
