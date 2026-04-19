using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Classification.DTOs;
using TalentSystem.Application.Features.Classification.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/talent-classifications")]
public sealed class TalentClassificationsController : ControllerBase
{
    private readonly ITalentClassificationService _service;

    public TalentClassificationsController(ITalentClassificationService service)
    {
        _service = service;
    }

    [HttpPost("classify")]
    public async Task<IActionResult> Classify(
        [FromBody] ClassifyTalentClassificationRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.ClassifyAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<TalentClassificationDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("reclassify")]
    public async Task<IActionResult> Reclassify(
        [FromBody] ReclassifyTalentClassificationRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.ReclassifyAsync(request, cancellationToken);
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
        [FromQuery] TalentClassificationFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<TalentClassificationDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }
}
