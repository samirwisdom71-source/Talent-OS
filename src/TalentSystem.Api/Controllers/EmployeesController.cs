using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Api.Extensions;
using TalentSystem.Application.Features.Employees.DTOs;
using TalentSystem.Application.Features.Employees.Interfaces;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _employeeService.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var payload = ApiResponse<EmployeeDto>.FromSuccess(result.Value!, traceId);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, payload);
        }

        return result.ToFailureActionResult(this, traceId);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        return result.ToEmployeeActionResult(this, traceId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        return result.ToEmployeeActionResult(this, traceId);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeListItemDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] EmployeeFilterRequest request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var result = await _employeeService.GetPagedAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<EmployeeListItemDto>>.FromSuccess(result.Value!, traceId));
        }

        return result.ToFailureActionResult(this, traceId);
    }
}
