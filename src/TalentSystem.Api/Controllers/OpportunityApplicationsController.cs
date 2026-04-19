using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/opportunity-applications")]
public sealed class OpportunityApplicationsController : ControllerBase
{
    private readonly IOpportunityApplicationService _service;

    public OpportunityApplicationsController(IOpportunityApplicationService service)
    {
        _service = service;
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyOpportunityRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.ApplyAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<OpportunityApplicationDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOpportunityApplicationRequest request,
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
        [FromQuery] OpportunityApplicationFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<OpportunityApplicationDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.WithdrawAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/under-review")]
    public async Task<IActionResult> MarkUnderReview(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.MarkUnderReviewAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/shortlist")]
    public async Task<IActionResult> Shortlist(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.ShortlistAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.AcceptAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.RejectAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
