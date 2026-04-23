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
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetUsersAsync(search, take, lang, cancellationToken);
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
        [FromQuery] Guid? organizationUnitId,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetPositionsAsync(search, take, organizationUnitId, cancellationToken);
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

    [HttpGet("job-grades")]
    public async Task<IActionResult> GetJobGrades(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetJobGradesAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("competencies")]
    public async Task<IActionResult> GetCompetencies(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetCompetenciesAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("competency-levels")]
    public async Task<IActionResult> GetCompetencyLevels(
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetCompetencyLevelsAsync(search, take, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("performance-evaluations")]
    public async Task<IActionResult> GetPerformanceEvaluations(
        [FromQuery] string? search,
        [FromQuery] int? take,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetPerformanceEvaluationsAsync(search, take, lang, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("talent-classifications")]
    public async Task<IActionResult> GetTalentClassifications(
        [FromQuery] string? search,
        [FromQuery] int? take,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetTalentClassificationsAsync(search, take, lang, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("development-plans")]
    public async Task<IActionResult> GetDevelopmentPlans(
        [FromQuery] string? search,
        [FromQuery] int? take,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetDevelopmentPlansAsync(search, take, lang, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("marketplace-opportunities")]
    public async Task<IActionResult> GetMarketplaceOpportunities(
        [FromQuery] string? search,
        [FromQuery] int? take,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetMarketplaceOpportunitiesAsync(search, take, lang, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("opportunity-applications")]
    public async Task<IActionResult> GetOpportunityApplications(
        [FromQuery] string? search,
        [FromQuery] int? take,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _identityLookupService.GetOpportunityApplicationsAsync(search, take, lang, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }
}
