using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Reports.DTOs;
using TalentSystem.Application.Features.Reports.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/system-reports")]
public sealed class SystemReportsController : ControllerBase
{
    private readonly ISystemReportService _systemReportService;
    private readonly ISystemReportExportService _exportService;

    public SystemReportsController(
        ISystemReportService systemReportService,
        ISystemReportExportService exportService)
    {
        _systemReportService = systemReportService;
        _exportService = exportService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemReportDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] SystemReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _systemReportService.BuildAsync(filter, cancellationToken);
        return result.ToApiActionResult(this, traceId);
    }

    [HttpGet("export/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemReportDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] SystemReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _systemReportService.BuildAsync(filter, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToFailureActionResult(this, traceId);
        }

        var bytes = _exportService.BuildPdf(result.Value!);
        var fileName = $"system-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemReportDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] SystemReportFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _systemReportService.BuildAsync(filter, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToFailureActionResult(this, traceId);
        }

        var bytes = _exportService.BuildExcel(result.Value!);
        var fileName = $"system-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
