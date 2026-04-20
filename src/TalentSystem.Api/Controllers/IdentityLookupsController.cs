using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Identity.Interfaces;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/identity/lookups")]
[Authorize]
public sealed class IdentityLookupsController : ControllerBase
{
    private readonly IIdentityLookupService _identityLookupService;

    public IdentityLookupsController(IIdentityLookupService identityLookupService)
    {
        _identityLookupService = identityLookupService;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetEmployeesAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetUsersAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetRolesAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetPermissionsAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetPositionsAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("organization-units")]
    public async Task<IActionResult> GetOrganizationUnits(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetOrganizationUnitsAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
