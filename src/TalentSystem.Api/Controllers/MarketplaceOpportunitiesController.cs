using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/marketplace-opportunities")]
public sealed class MarketplaceOpportunitiesController : ControllerBase
{
    private readonly IMarketplaceOpportunityService _service;

    public MarketplaceOpportunitiesController(IMarketplaceOpportunityService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMarketplaceOpportunityRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<MarketplaceOpportunityDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateMarketplaceOpportunityRequest request,
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
        [FromQuery] MarketplaceOpportunityFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<MarketplaceOpportunityDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/open")]
    public async Task<IActionResult> Open(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.OpenAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.CloseAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _service.CancelAsync(id, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
