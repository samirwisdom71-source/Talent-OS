using System.Diagnostics;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSystem.Application.Features.Competencies.DTOs;
using TalentSystem.Application.Features.Competencies.Interfaces;
using TalentSystem.Application.Features.Development.DTOs;
using TalentSystem.Application.Features.Development.Interfaces;
using TalentSystem.Application.Features.Employees.DTOs;
using TalentSystem.Application.Features.Employees.Interfaces;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Application.Features.JobArchitecture.DTOs;
using TalentSystem.Application.Features.JobArchitecture.Interfaces;
using TalentSystem.Application.Features.Marketplace.DTOs;
using TalentSystem.Application.Features.Marketplace.Interfaces;
using TalentSystem.Application.Features.Organizations.DTOs;
using TalentSystem.Application.Features.Organizations.Interfaces;
using TalentSystem.Application.Features.Performance.DTOs;
using TalentSystem.Application.Features.Performance.Interfaces;
using TalentSystem.Application.Features.Succession.DTOs;
using TalentSystem.Application.Features.Succession.Interfaces;
using TalentSystem.Domain.Enums;
using TalentSystem.Shared.Api;

namespace TalentSystem.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private static readonly string[] SupportedTables =
    [
        "organization-units",
        "job-grades",
        "positions",
        "competency-categories",
        "competency-levels",
        "competencies",
        "employees",
        "roles",
        "users",
        "performance-cycles",
        "critical-positions",
        "job-competency-requirements",
        "succession-plans",
        "development-plans",
        "marketplace-opportunities"
    ];

    private readonly IOrganizationUnitService _organizationUnitService;
    private readonly IJobGradeService _jobGradeService;
    private readonly IPositionService _positionService;
    private readonly ICompetencyCategoryService _competencyCategoryService;
    private readonly ICompetencyLevelService _competencyLevelService;
    private readonly ICompetencyService _competencyService;
    private readonly IEmployeeService _employeeService;
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;
    private readonly ICriticalPositionService _criticalPositionService;
    private readonly IJobCompetencyRequirementService _jobCompetencyRequirementService;
    private readonly ISuccessionPlanService _successionPlanService;
    private readonly IDevelopmentPlanService _developmentPlanService;
    private readonly IMarketplaceOpportunityService _marketplaceOpportunityService;
    private readonly IPerformanceCycleService _performanceCycleService;

    public SystemController(
        IOrganizationUnitService organizationUnitService,
        IJobGradeService jobGradeService,
        IPositionService positionService,
        ICompetencyCategoryService competencyCategoryService,
        ICompetencyLevelService competencyLevelService,
        ICompetencyService competencyService,
        IEmployeeService employeeService,
        IRoleService roleService,
        IUserService userService,
        ICriticalPositionService criticalPositionService,
        IJobCompetencyRequirementService jobCompetencyRequirementService,
        ISuccessionPlanService successionPlanService,
        IDevelopmentPlanService developmentPlanService,
        IMarketplaceOpportunityService marketplaceOpportunityService,
        IPerformanceCycleService performanceCycleService)
    {
        _organizationUnitService = organizationUnitService;
        _jobGradeService = jobGradeService;
        _positionService = positionService;
        _competencyCategoryService = competencyCategoryService;
        _competencyLevelService = competencyLevelService;
        _competencyService = competencyService;
        _employeeService = employeeService;
        _roleService = roleService;
        _userService = userService;
        _criticalPositionService = criticalPositionService;
        _jobCompetencyRequirementService = jobCompetencyRequirementService;
        _successionPlanService = successionPlanService;
        _developmentPlanService = developmentPlanService;
        _marketplaceOpportunityService = marketplaceOpportunityService;
        _performanceCycleService = performanceCycleService;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string>> GetHealth()
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return Ok(ApiResponse<string>.FromSuccess("Healthy", traceId));
    }

    [HttpGet("excel-template")]
    public IActionResult DownloadExcelTemplate(
        [FromQuery] string? table = "all",
        [FromQuery] bool withSampleData = false)
    {
        var normalized = NormalizeTableSelection(table);
        if (!IsValidTableSelection(normalized))
        {
            return BadRequest(ApiResponse<object>.FromFailure(
                new[] { "Invalid table selection. Use 'all' or one of: " + string.Join(", ", SupportedTables) },
                Activity.Current?.Id ?? HttpContext.TraceIdentifier));
        }

        using var workbook = new XLWorkbook();
        foreach (var t in ResolveTableList(normalized))
        {
            AddTemplateSheet(workbook, t);
            if (withSampleData)
            {
                AddSampleRows(workbook.Worksheet(t), t);
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var mode = withSampleData ? "sample" : "blank";
        var fileName = normalized == "all"
            ? $"talent-os-import-template-all-{mode}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            : $"talent-os-import-template-{normalized}-{mode}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost("excel-import")]
    [RequestSizeLimit(20_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportExcel(
        [FromForm] ImportExcelRequest request,
        CancellationToken cancellationToken = default)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var file = request.File;
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.FromFailure(new[] { "Excel file is required." }, traceId));
        }

        var normalized = NormalizeTableSelection(request.Table);
        if (!IsValidTableSelection(normalized))
        {
            return BadRequest(ApiResponse<object>.FromFailure(
                new[] { "Invalid table selection. Use 'all' or one of: " + string.Join(", ", SupportedTables) },
                traceId));
        }

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.FromFailure(new[] { "Only .xlsx files are supported." }, traceId));
        }

        try
        {
            await using var fileStream = file.OpenReadStream();
            using var workbook = new XLWorkbook(fileStream);
            var tables = ResolveTableList(normalized);
            var results = new List<ExcelImportTableResultDto>(tables.Count);

            var organizationUnitsByNameAr = await LoadOrganizationUnitsByNameAr(cancellationToken);
            var organizationUnitsByNameEn = await LoadOrganizationUnitsByNameEn(cancellationToken);
            var jobGradesByName = await LoadJobGradesByName(cancellationToken);
            var positionsByTitleAr = await LoadPositionsByTitleArMap(cancellationToken);
            var positionsByTitleEn = await LoadPositionsByTitleEnMap(cancellationToken);
            var categoriesByNameAr = await LoadCompetencyCategoriesByNameAr(cancellationToken);
            var categoriesByNameEn = await LoadCompetencyCategoriesByNameEn(cancellationToken);
            var competenciesByCode = await LoadCompetenciesByCode(cancellationToken);
            var competencyLevelsByName = await LoadCompetencyLevelsByName(cancellationToken);
            var rolesByNameAr = await LoadRolesByNameAr(cancellationToken);
            var rolesByNameEn = await LoadRolesByNameEn(cancellationToken);
            var employeesByNumber = await LoadEmployeesByNumber(cancellationToken);
            var performanceCyclesByNameAr = await LoadPerformanceCyclesByNameAr(cancellationToken);
            var performanceCyclesByNameEn = await LoadPerformanceCyclesByNameEn(cancellationToken);
            var criticalPositionByPositionId = await LoadCriticalPositionByPositionId(cancellationToken);
            var opportunitiesByTitle = await LoadOpportunitiesByTitle(cancellationToken);

            foreach (var selectedTable in tables)
            {
                if (!workbook.TryGetWorksheet(selectedTable, out var worksheet))
                {
                    results.Add(new ExcelImportTableResultDto(selectedTable, 0, 0, new[] { "Sheet not found in uploaded file." }));
                    continue;
                }

                var tableResult = selectedTable switch
                {
                    "organization-units" => await ImportOrganizationUnits(worksheet!, organizationUnitsByNameAr, organizationUnitsByNameEn, cancellationToken),
                    "job-grades" => await ImportJobGrades(worksheet!, jobGradesByName, cancellationToken),
                    "positions" => await ImportPositions(worksheet!, organizationUnitsByNameAr, organizationUnitsByNameEn, jobGradesByName, positionsByTitleAr, positionsByTitleEn, cancellationToken),
                    "competency-categories" => await ImportCompetencyCategories(worksheet!, categoriesByNameAr, categoriesByNameEn, cancellationToken),
                    "competency-levels" => await ImportCompetencyLevels(worksheet!, competencyLevelsByName, cancellationToken),
                    "competencies" => await ImportCompetencies(worksheet!, categoriesByNameAr, categoriesByNameEn, competenciesByCode, cancellationToken),
                    "employees" => await ImportEmployees(worksheet!, organizationUnitsByNameAr, organizationUnitsByNameEn, positionsByTitleAr, positionsByTitleEn, employeesByNumber, cancellationToken),
                    "roles" => await ImportRoles(worksheet!, rolesByNameAr, rolesByNameEn, cancellationToken),
                    "users" => await ImportUsers(worksheet!, employeesByNumber, rolesByNameAr, rolesByNameEn, cancellationToken),
                    "performance-cycles" => await ImportPerformanceCycles(worksheet!, performanceCyclesByNameAr, performanceCyclesByNameEn, cancellationToken),
                    "critical-positions" => await ImportCriticalPositions(worksheet!, positionsByTitleAr, positionsByTitleEn, criticalPositionByPositionId, cancellationToken),
                    "job-competency-requirements" => await ImportJobCompetencyRequirements(worksheet!, positionsByTitleAr, positionsByTitleEn, competenciesByCode, competencyLevelsByName, cancellationToken),
                    "succession-plans" => await ImportSuccessionPlans(worksheet!, positionsByTitleAr, positionsByTitleEn, criticalPositionByPositionId, performanceCyclesByNameAr, performanceCyclesByNameEn, cancellationToken),
                    "development-plans" => await ImportDevelopmentPlans(worksheet!, employeesByNumber, performanceCyclesByNameAr, performanceCyclesByNameEn, cancellationToken),
                    "marketplace-opportunities" => await ImportMarketplaceOpportunities(worksheet!, organizationUnitsByNameAr, organizationUnitsByNameEn, positionsByTitleAr, positionsByTitleEn, opportunitiesByTitle, cancellationToken),
                    _ => new ExcelImportTableResultDto(selectedTable, 0, 0, new[] { "Unsupported table." })
                };

                results.Add(tableResult);
            }

            return Ok(ApiResponse<ExcelImportResponseDto>.FromSuccess(new ExcelImportResponseDto(results), traceId));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FromFailure(new[] { $"Import failed: {ex.Message}" }, traceId));
        }
    }

    private async Task<ExcelImportTableResultDto> ImportOrganizationUnits(
        IXLWorksheet sheet,
        Dictionary<string, Guid> byNameAr,
        Dictionary<string, Guid> byNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var parentNameAr = GetString(row, headers, "ParentNameAr");
            var parentNameEn = GetString(row, headers, "ParentNameEn");

            if (string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
            {
                skipped++;
                continue;
            }

            if (byNameAr.ContainsKey(nameAr) || byNameEn.ContainsKey(nameEn))
            {
                skipped++;
                continue;
            }

            Guid? parentId = null;
            if (!string.IsNullOrWhiteSpace(parentNameAr) || !string.IsNullOrWhiteSpace(parentNameEn))
            {
                if (!TryResolveReferenceId(parentNameAr, parentNameEn, byNameAr, byNameEn, out var resolvedParentId))
                {
                    errors.Add($"organization-units row {row.RowNumber()}: parent unit not found.");
                    continue;
                }

                parentId = resolvedParentId;
            }

            var result = await _organizationUnitService.CreateAsync(new CreateOrganizationUnitRequest
            {
                NameAr = nameAr,
                NameEn = nameEn,
                ParentId = parentId
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                byNameAr[nameAr] = result.Value!.Id;
                byNameEn[nameEn] = result.Value!.Id;
            }
            else
            {
                errors.Add($"organization-units row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("organization-units", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportJobGrades(
        IXLWorksheet sheet,
        Dictionary<string, Guid> byName,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var name = GetString(row, headers, "Name");
            var levelText = GetString(row, headers, "Level");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(levelText))
            {
                skipped++;
                continue;
            }

            if (byName.ContainsKey(name))
            {
                skipped++;
                continue;
            }

            if (!int.TryParse(levelText, out var level))
            {
                errors.Add($"job-grades row {row.RowNumber()}: invalid Level.");
                continue;
            }

            var result = await _jobGradeService.CreateAsync(new CreateJobGradeRequest
            {
                Name = name,
                Level = level
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                byName[name] = result.Value!.Id;
            }
            else
            {
                errors.Add($"job-grades row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("job-grades", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportPositions(
        IXLWorksheet sheet,
        Dictionary<string, Guid> orgByNameAr,
        Dictionary<string, Guid> orgByNameEn,
        Dictionary<string, Guid> gradesByName,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existingPositions = await LoadPositionsByTitleAr(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var titleAr = GetString(row, headers, "TitleAr");
            var titleEn = GetString(row, headers, "TitleEn");
            var orgNameAr = GetString(row, headers, "OrganizationUnitNameAr");
            var orgNameEn = GetString(row, headers, "OrganizationUnitNameEn");
            var jobGradeName = GetString(row, headers, "JobGradeName");

            if (string.IsNullOrWhiteSpace(titleAr) || string.IsNullOrWhiteSpace(titleEn))
            {
                skipped++;
                continue;
            }

            if (existingPositions.Contains(titleAr))
            {
                skipped++;
                continue;
            }

            if (!TryResolveReferenceId(orgNameAr, orgNameEn, orgByNameAr, orgByNameEn, out var resolvedOrgId))
            {
                errors.Add($"positions row {row.RowNumber()}: organization unit not found.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(jobGradeName) || !gradesByName.TryGetValue(jobGradeName, out var gradeId))
            {
                errors.Add($"positions row {row.RowNumber()}: job grade not found.");
                continue;
            }

            var result = await _positionService.CreateAsync(new CreatePositionRequest
            {
                TitleAr = titleAr,
                TitleEn = titleEn,
                OrganizationUnitId = resolvedOrgId,
                JobGradeId = gradeId
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existingPositions.Add(titleAr);
                AddOrUpdateLookup(positionsByTitleAr, titleAr, result.Value!.Id);
                AddOrUpdateLookup(positionsByTitleEn, titleEn, result.Value!.Id);
            }
            else
            {
                errors.Add($"positions row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("positions", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportCompetencyCategories(
        IXLWorksheet sheet,
        Dictionary<string, Guid> byNameAr,
        Dictionary<string, Guid> byNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var description = GetString(row, headers, "Description");

            if (string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
            {
                skipped++;
                continue;
            }

            if (byNameAr.ContainsKey(nameAr) || byNameEn.ContainsKey(nameEn))
            {
                skipped++;
                continue;
            }

            var result = await _competencyCategoryService.CreateAsync(new CreateCompetencyCategoryRequest
            {
                NameAr = nameAr,
                NameEn = nameEn,
                Description = string.IsNullOrWhiteSpace(description) ? null : description
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                byNameAr[nameAr] = result.Value!.Id;
                byNameEn[nameEn] = result.Value!.Id;
            }
            else
            {
                errors.Add($"competency-categories row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("competency-categories", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportCompetencyLevels(
        IXLWorksheet sheet,
        Dictionary<string, Guid> competencyLevelsByName,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existingNames = await LoadCompetencyLevelNames(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var name = GetString(row, headers, "Name");
            var valueText = GetString(row, headers, "NumericValue");
            var description = GetString(row, headers, "Description");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(valueText))
            {
                skipped++;
                continue;
            }

            if (existingNames.Contains(name))
            {
                skipped++;
                continue;
            }

            if (!int.TryParse(valueText, out var numericValue))
            {
                errors.Add($"competency-levels row {row.RowNumber()}: invalid NumericValue.");
                continue;
            }

            var result = await _competencyLevelService.CreateAsync(new CreateCompetencyLevelRequest
            {
                Name = name,
                NumericValue = numericValue,
                Description = string.IsNullOrWhiteSpace(description) ? null : description
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existingNames.Add(name);
                AddOrUpdateLookup(competencyLevelsByName, name, result.Value!.Id);
            }
            else
            {
                errors.Add($"competency-levels row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("competency-levels", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportCompetencies(
        IXLWorksheet sheet,
        Dictionary<string, Guid> categoriesByNameAr,
        Dictionary<string, Guid> categoriesByNameEn,
        Dictionary<string, Guid> competenciesByCode,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existingCodes = await LoadCompetencyCodes(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var code = GetString(row, headers, "Code");
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var description = GetString(row, headers, "Description");
            var categoryNameAr = GetString(row, headers, "CategoryNameAr");
            var categoryNameEn = GetString(row, headers, "CategoryNameEn");

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
            {
                skipped++;
                continue;
            }

            if (existingCodes.Contains(code))
            {
                skipped++;
                continue;
            }

            if (!TryResolveReferenceId(categoryNameAr, categoryNameEn, categoriesByNameAr, categoriesByNameEn, out var resolvedCategoryId))
            {
                errors.Add($"competencies row {row.RowNumber()}: category not found.");
                continue;
            }

            var result = await _competencyService.CreateAsync(new CreateCompetencyRequest
            {
                Code = code,
                NameAr = nameAr,
                NameEn = nameEn,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                CompetencyCategoryId = resolvedCategoryId
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existingCodes.Add(code);
                AddOrUpdateLookup(competenciesByCode, code, result.Value!.Id);
            }
            else
            {
                errors.Add($"competencies row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("competencies", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportEmployees(
        IXLWorksheet sheet,
        Dictionary<string, Guid> orgByNameAr,
        Dictionary<string, Guid> orgByNameEn,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        Dictionary<string, Guid> employeesByNumber,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var employeeNumber = GetString(row, headers, "EmployeeNumber");
            var fullNameAr = GetString(row, headers, "FullNameAr");
            var fullNameEn = GetString(row, headers, "FullNameEn");
            var email = GetString(row, headers, "Email");
            var orgNameAr = GetString(row, headers, "OrganizationUnitNameAr");
            var orgNameEn = GetString(row, headers, "OrganizationUnitNameEn");
            var positionTitleAr = GetString(row, headers, "PositionTitleAr");
            var positionTitleEn = GetString(row, headers, "PositionTitleEn");

            if (string.IsNullOrWhiteSpace(employeeNumber) ||
                string.IsNullOrWhiteSpace(fullNameAr) ||
                string.IsNullOrWhiteSpace(fullNameEn) ||
                string.IsNullOrWhiteSpace(email))
            {
                skipped++;
                continue;
            }

            if (ContainsLookup(employeesByNumber, employeeNumber))
            {
                skipped++;
                continue;
            }

            if (!TryResolveReferenceId(orgNameAr, orgNameEn, orgByNameAr, orgByNameEn, out var organizationUnitId))
            {
                errors.Add($"employees row {row.RowNumber()}: organization unit not found.");
                continue;
            }

            if (!TryResolveReferenceId(positionTitleAr, positionTitleEn, positionsByTitleAr, positionsByTitleEn, out var positionId))
            {
                errors.Add($"employees row {row.RowNumber()}: position not found.");
                continue;
            }

            var result = await _employeeService.CreateAsync(new CreateEmployeeRequest
            {
                EmployeeNumber = employeeNumber,
                FullNameAr = fullNameAr,
                FullNameEn = fullNameEn,
                Email = email,
                OrganizationUnitId = organizationUnitId,
                PositionId = positionId
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                AddOrUpdateLookup(employeesByNumber, employeeNumber, result.Value!.Id);
            }
            else
            {
                errors.Add($"employees row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("employees", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportRoles(
        IXLWorksheet sheet,
        Dictionary<string, Guid> rolesByNameAr,
        Dictionary<string, Guid> rolesByNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var descriptionAr = GetString(row, headers, "DescriptionAr");
            var descriptionEn = GetString(row, headers, "DescriptionEn");
            var isSystemRoleText = GetString(row, headers, "IsSystemRole");

            if (string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
            {
                skipped++;
                continue;
            }

            if (rolesByNameAr.ContainsKey(nameAr) || rolesByNameEn.ContainsKey(nameEn))
            {
                skipped++;
                continue;
            }

            var isSystemRole = string.Equals(isSystemRoleText, "true", StringComparison.OrdinalIgnoreCase)
                || isSystemRoleText == "1"
                || string.Equals(isSystemRoleText, "yes", StringComparison.OrdinalIgnoreCase);

            var result = await _roleService.CreateAsync(new CreateRoleRequest
            {
                NameAr = nameAr,
                NameEn = nameEn,
                DescriptionAr = string.IsNullOrWhiteSpace(descriptionAr) ? null : descriptionAr,
                DescriptionEn = string.IsNullOrWhiteSpace(descriptionEn) ? null : descriptionEn,
                IsSystemRole = isSystemRole
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                rolesByNameAr[nameAr] = result.Value!.Id;
                rolesByNameEn[nameEn] = result.Value!.Id;
            }
            else
            {
                errors.Add($"roles row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("roles", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportUsers(
        IXLWorksheet sheet,
        Dictionary<string, Guid> employeesByNumber,
        Dictionary<string, Guid> rolesByNameAr,
        Dictionary<string, Guid> rolesByNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existingUsers = await LoadUsersByUserName(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var userName = GetString(row, headers, "UserName");
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var email = GetString(row, headers, "Email");
            var password = GetString(row, headers, "Password");
            var employeeNumber = GetString(row, headers, "EmployeeNumber");
            var roleNames = GetString(row, headers, "RoleNames");

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                skipped++;
                continue;
            }

            if (existingUsers.Contains(userName))
            {
                skipped++;
                continue;
            }

            Guid? employeeId = null;
            if (!string.IsNullOrWhiteSpace(employeeNumber))
            {
                if (!TryGetFromLookup(employeesByNumber, employeeNumber, out var resolvedEmployeeId))
                {
                    errors.Add($"users row {row.RowNumber()}: employee number not found.");
                    continue;
                }

                employeeId = resolvedEmployeeId;
            }

            var roleIds = new List<Guid>();
            foreach (var roleName in roleNames
                         .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (!TryResolveReferenceId(roleName, roleName, rolesByNameAr, rolesByNameEn, out var roleId))
                {
                    errors.Add($"users row {row.RowNumber()}: role '{roleName}' not found.");
                    roleIds.Clear();
                    break;
                }

                roleIds.Add(roleId);
            }

            if (roleNames.Length > 0 && roleIds.Count == 0)
            {
                continue;
            }

            var result = await _userService.CreateAsync(new CreateUserRequest
            {
                UserName = userName,
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr,
                NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn,
                Email = email,
                Password = password,
                EmployeeId = employeeId,
                RoleIds = roleIds
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existingUsers.Add(userName);
            }
            else
            {
                errors.Add($"users row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("users", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportPerformanceCycles(
        IXLWorksheet sheet,
        Dictionary<string, Guid> cyclesByNameAr,
        Dictionary<string, Guid> cyclesByNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var nameAr = GetString(row, headers, "NameAr");
            var nameEn = GetString(row, headers, "NameEn");
            var startDateText = GetString(row, headers, "StartDate");
            var endDateText = GetString(row, headers, "EndDate");
            var description = GetString(row, headers, "Description");

            if (string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
            {
                skipped++;
                continue;
            }

            if (ContainsLookup(cyclesByNameAr, nameAr) || ContainsLookup(cyclesByNameEn, nameEn))
            {
                skipped++;
                continue;
            }

            if (!DateTime.TryParse(startDateText, out var startDate) || !DateTime.TryParse(endDateText, out var endDate))
            {
                errors.Add($"performance-cycles row {row.RowNumber()}: invalid StartDate/EndDate.");
                continue;
            }

            var result = await _performanceCycleService.CreateAsync(new CreatePerformanceCycleRequest
            {
                NameAr = nameAr,
                NameEn = nameEn,
                StartDate = startDate,
                EndDate = endDate,
                Description = string.IsNullOrWhiteSpace(description) ? null : description
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                AddOrUpdateLookup(cyclesByNameAr, nameAr, result.Value!.Id);
                AddOrUpdateLookup(cyclesByNameEn, nameEn, result.Value!.Id);
            }
            else
            {
                errors.Add($"performance-cycles row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("performance-cycles", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportCriticalPositions(
        IXLWorksheet sheet,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        Dictionary<Guid, Guid> criticalPositionByPositionId,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var positionTitleAr = GetString(row, headers, "PositionTitleAr");
            var positionTitleEn = GetString(row, headers, "PositionTitleEn");
            var criticality = GetString(row, headers, "CriticalityLevel");
            var risk = GetString(row, headers, "RiskLevel");
            var notes = GetString(row, headers, "Notes");

            if (!TryResolveReferenceId(positionTitleAr, positionTitleEn, positionsByTitleAr, positionsByTitleEn, out var positionId))
            {
                errors.Add($"critical-positions row {row.RowNumber()}: position not found.");
                continue;
            }

            if (criticalPositionByPositionId.ContainsKey(positionId))
            {
                skipped++;
                continue;
            }

            if (!TryParseEnum<CriticalityLevel>(criticality, out var criticalityLevel) ||
                !TryParseEnum<SuccessionRiskLevel>(risk, out var riskLevel))
            {
                errors.Add($"critical-positions row {row.RowNumber()}: invalid enum values.");
                continue;
            }

            var result = await _criticalPositionService.CreateAsync(new CreateCriticalPositionRequest
            {
                PositionId = positionId,
                CriticalityLevel = criticalityLevel,
                RiskLevel = riskLevel,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                criticalPositionByPositionId[positionId] = result.Value!.Id;
            }
            else
            {
                errors.Add($"critical-positions row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("critical-positions", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportJobCompetencyRequirements(
        IXLWorksheet sheet,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        Dictionary<string, Guid> competenciesByCode,
        Dictionary<string, Guid> competencyLevelsByName,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existing = await LoadJobCompetencyRequirementKeys(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var positionTitleAr = GetString(row, headers, "PositionTitleAr");
            var positionTitleEn = GetString(row, headers, "PositionTitleEn");
            var competencyCode = GetString(row, headers, "CompetencyCode");
            var requiredLevelName = GetString(row, headers, "RequiredLevelName");

            if (!TryResolveReferenceId(positionTitleAr, positionTitleEn, positionsByTitleAr, positionsByTitleEn, out var positionId))
            {
                errors.Add($"job-competency-requirements row {row.RowNumber()}: position not found.");
                continue;
            }

            if (!TryGetFromLookup(competenciesByCode, competencyCode, out var competencyId))
            {
                errors.Add($"job-competency-requirements row {row.RowNumber()}: competency code not found.");
                continue;
            }

            if (!TryGetFromLookup(competencyLevelsByName, requiredLevelName, out var requiredLevelId))
            {
                errors.Add($"job-competency-requirements row {row.RowNumber()}: required level not found.");
                continue;
            }

            var key = $"{positionId}:{competencyId}:{requiredLevelId}";
            if (existing.Contains(key))
            {
                skipped++;
                continue;
            }

            var result = await _jobCompetencyRequirementService.CreateAsync(new CreateJobCompetencyRequirementRequest
            {
                PositionId = positionId,
                CompetencyId = competencyId,
                RequiredLevelId = requiredLevelId
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existing.Add(key);
            }
            else
            {
                errors.Add($"job-competency-requirements row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("job-competency-requirements", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportSuccessionPlans(
        IXLWorksheet sheet,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        Dictionary<Guid, Guid> criticalPositionByPositionId,
        Dictionary<string, Guid> performanceCyclesByNameAr,
        Dictionary<string, Guid> performanceCyclesByNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existing = await LoadSuccessionPlanKeys(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var planName = GetString(row, headers, "PlanName");
            var notes = GetString(row, headers, "Notes");
            var positionTitleAr = GetString(row, headers, "PositionTitleAr");
            var positionTitleEn = GetString(row, headers, "PositionTitleEn");
            var cycleNameAr = GetString(row, headers, "PerformanceCycleNameAr");
            var cycleNameEn = GetString(row, headers, "PerformanceCycleNameEn");

            if (string.IsNullOrWhiteSpace(planName))
            {
                skipped++;
                continue;
            }

            if (!TryResolveReferenceId(positionTitleAr, positionTitleEn, positionsByTitleAr, positionsByTitleEn, out var positionId))
            {
                errors.Add($"succession-plans row {row.RowNumber()}: position not found.");
                continue;
            }

            if (!criticalPositionByPositionId.TryGetValue(positionId, out var criticalPositionId))
            {
                errors.Add($"succession-plans row {row.RowNumber()}: critical position is missing for this position.");
                continue;
            }

            if (!TryResolveReferenceId(cycleNameAr, cycleNameEn, performanceCyclesByNameAr, performanceCyclesByNameEn, out var performanceCycleId))
            {
                errors.Add($"succession-plans row {row.RowNumber()}: performance cycle not found.");
                continue;
            }

            var key = $"{criticalPositionId}:{performanceCycleId}:{planName}";
            if (existing.Contains(key))
            {
                skipped++;
                continue;
            }

            var result = await _successionPlanService.CreateAsync(new CreateSuccessionPlanRequest
            {
                CriticalPositionId = criticalPositionId,
                PerformanceCycleId = performanceCycleId,
                PlanName = planName,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existing.Add(key);
            }
            else
            {
                errors.Add($"succession-plans row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("succession-plans", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportDevelopmentPlans(
        IXLWorksheet sheet,
        Dictionary<string, Guid> employeesByNumber,
        Dictionary<string, Guid> performanceCyclesByNameAr,
        Dictionary<string, Guid> performanceCyclesByNameEn,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();
        var existing = await LoadDevelopmentPlanKeys(cancellationToken);

        foreach (var row in ReadDataRows(sheet))
        {
            var employeeNumber = GetString(row, headers, "EmployeeNumber");
            var cycleNameAr = GetString(row, headers, "PerformanceCycleNameAr");
            var cycleNameEn = GetString(row, headers, "PerformanceCycleNameEn");
            var planTitle = GetString(row, headers, "PlanTitle");
            var sourceTypeText = GetString(row, headers, "SourceType");
            var targetCompletionDateText = GetString(row, headers, "TargetCompletionDate");
            var notes = GetString(row, headers, "Notes");

            if (string.IsNullOrWhiteSpace(planTitle))
            {
                skipped++;
                continue;
            }

            if (!TryGetFromLookup(employeesByNumber, employeeNumber, out var employeeId))
            {
                errors.Add($"development-plans row {row.RowNumber()}: employee number not found.");
                continue;
            }

            if (!TryResolveReferenceId(cycleNameAr, cycleNameEn, performanceCyclesByNameAr, performanceCyclesByNameEn, out var performanceCycleId))
            {
                errors.Add($"development-plans row {row.RowNumber()}: performance cycle not found.");
                continue;
            }

            if (!TryParseEnum<DevelopmentPlanSourceType>(sourceTypeText, out var sourceType))
            {
                errors.Add($"development-plans row {row.RowNumber()}: invalid SourceType.");
                continue;
            }

            var key = $"{employeeId}:{performanceCycleId}:{planTitle}";
            if (existing.Contains(key))
            {
                skipped++;
                continue;
            }

            DateTime? targetCompletionDate = null;
            if (!string.IsNullOrWhiteSpace(targetCompletionDateText) && DateTime.TryParse(targetCompletionDateText, out var parsedDate))
            {
                targetCompletionDate = parsedDate;
            }

            var result = await _developmentPlanService.CreateAsync(new CreateDevelopmentPlanRequest
            {
                EmployeeId = employeeId,
                PerformanceCycleId = performanceCycleId,
                PlanTitle = planTitle,
                SourceType = sourceType,
                TargetCompletionDate = targetCompletionDate,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                existing.Add(key);
            }
            else
            {
                errors.Add($"development-plans row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("development-plans", inserted, skipped, errors);
    }

    private async Task<ExcelImportTableResultDto> ImportMarketplaceOpportunities(
        IXLWorksheet sheet,
        Dictionary<string, Guid> orgByNameAr,
        Dictionary<string, Guid> orgByNameEn,
        Dictionary<string, Guid> positionsByTitleAr,
        Dictionary<string, Guid> positionsByTitleEn,
        Dictionary<string, Guid> opportunitiesByTitle,
        CancellationToken cancellationToken)
    {
        var headers = ReadHeaders(sheet);
        var inserted = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var row in ReadDataRows(sheet))
        {
            var title = GetString(row, headers, "Title");
            var description = GetString(row, headers, "Description");
            var typeText = GetString(row, headers, "OpportunityType");
            var orgNameAr = GetString(row, headers, "OrganizationUnitNameAr");
            var orgNameEn = GetString(row, headers, "OrganizationUnitNameEn");
            var positionTitleAr = GetString(row, headers, "PositionTitleAr");
            var positionTitleEn = GetString(row, headers, "PositionTitleEn");
            var requiredCompetencySummary = GetString(row, headers, "RequiredCompetencySummary");
            var openDateText = GetString(row, headers, "OpenDate");
            var closeDateText = GetString(row, headers, "CloseDate");
            var maxApplicantsText = GetString(row, headers, "MaxApplicants");
            var isConfidentialText = GetString(row, headers, "IsConfidential");
            var notes = GetString(row, headers, "Notes");

            if (string.IsNullOrWhiteSpace(title))
            {
                skipped++;
                continue;
            }

            if (ContainsLookup(opportunitiesByTitle, title))
            {
                skipped++;
                continue;
            }

            if (!TryResolveReferenceId(orgNameAr, orgNameEn, orgByNameAr, orgByNameEn, out var orgId))
            {
                errors.Add($"marketplace-opportunities row {row.RowNumber()}: organization unit not found.");
                continue;
            }

            if (!TryParseEnum<OpportunityType>(typeText, out var opportunityType))
            {
                errors.Add($"marketplace-opportunities row {row.RowNumber()}: invalid OpportunityType.");
                continue;
            }

            if (!DateTime.TryParse(openDateText, out var openDate))
            {
                errors.Add($"marketplace-opportunities row {row.RowNumber()}: invalid OpenDate.");
                continue;
            }

            Guid? positionId = null;
            if (!string.IsNullOrWhiteSpace(positionTitleAr) || !string.IsNullOrWhiteSpace(positionTitleEn))
            {
                if (!TryResolveReferenceId(positionTitleAr, positionTitleEn, positionsByTitleAr, positionsByTitleEn, out var resolvedPositionId))
                {
                    errors.Add($"marketplace-opportunities row {row.RowNumber()}: position not found.");
                    continue;
                }

                positionId = resolvedPositionId;
            }

            DateTime? closeDate = null;
            if (!string.IsNullOrWhiteSpace(closeDateText) && DateTime.TryParse(closeDateText, out var parsedCloseDate))
            {
                closeDate = parsedCloseDate;
            }

            int? maxApplicants = null;
            if (!string.IsNullOrWhiteSpace(maxApplicantsText) && int.TryParse(maxApplicantsText, out var parsedMaxApplicants))
            {
                maxApplicants = parsedMaxApplicants;
            }

            var isConfidential = string.Equals(isConfidentialText, "true", StringComparison.OrdinalIgnoreCase)
                || isConfidentialText == "1"
                || string.Equals(isConfidentialText, "yes", StringComparison.OrdinalIgnoreCase);

            var result = await _marketplaceOpportunityService.CreateAsync(new CreateMarketplaceOpportunityRequest
            {
                Title = title,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                OpportunityType = opportunityType,
                OrganizationUnitId = orgId,
                PositionId = positionId,
                RequiredCompetencySummary = string.IsNullOrWhiteSpace(requiredCompetencySummary) ? null : requiredCompetencySummary,
                OpenDate = openDate,
                CloseDate = closeDate,
                MaxApplicants = maxApplicants,
                IsConfidential = isConfidential,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
            }, cancellationToken);

            if (result.IsSuccess)
            {
                inserted++;
                AddOrUpdateLookup(opportunitiesByTitle, title, result.Value!.Id);
            }
            else
            {
                errors.Add($"marketplace-opportunities row {row.RowNumber()}: {string.Join(" | ", result.Errors)}");
            }
        }

        return new ExcelImportTableResultDto("marketplace-opportunities", inserted, skipped, errors);
    }

    private static string NormalizeTableSelection(string? table) =>
        string.IsNullOrWhiteSpace(table) ? "all" : table.Trim().ToLowerInvariant();

    private static bool IsValidTableSelection(string table) =>
        table == "all" || SupportedTables.Contains(table, StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> ResolveTableList(string table) =>
        table == "all" ? SupportedTables : new[] { table };

    private static void AddTemplateSheet(XLWorkbook workbook, string table)
    {
        var sheet = workbook.Worksheets.Add(table);
        var headers = table switch
        {
            "organization-units" => new[] { "NameAr", "NameEn", "ParentNameAr", "ParentNameEn" },
            "job-grades" => new[] { "Name", "Level" },
            "positions" => new[] { "TitleAr", "TitleEn", "OrganizationUnitNameAr", "OrganizationUnitNameEn", "JobGradeName" },
            "competency-categories" => new[] { "NameAr", "NameEn", "Description" },
            "competency-levels" => new[] { "Name", "NumericValue", "Description" },
            "competencies" => new[] { "Code", "NameAr", "NameEn", "Description", "CategoryNameAr", "CategoryNameEn" },
            "employees" => new[] { "EmployeeNumber", "FullNameAr", "FullNameEn", "Email", "OrganizationUnitNameAr", "OrganizationUnitNameEn", "PositionTitleAr", "PositionTitleEn" },
            "roles" => new[] { "NameAr", "NameEn", "DescriptionAr", "DescriptionEn", "IsSystemRole" },
            "users" => new[] { "UserName", "NameAr", "NameEn", "Email", "Password", "EmployeeNumber", "RoleNames" },
            "performance-cycles" => new[] { "NameAr", "NameEn", "StartDate", "EndDate", "Description" },
            "critical-positions" => new[] { "PositionTitleAr", "PositionTitleEn", "CriticalityLevel", "RiskLevel", "Notes" },
            "job-competency-requirements" => new[] { "PositionTitleAr", "PositionTitleEn", "CompetencyCode", "RequiredLevelName" },
            "succession-plans" => new[] { "PlanName", "PositionTitleAr", "PositionTitleEn", "PerformanceCycleNameAr", "PerformanceCycleNameEn", "Notes" },
            "development-plans" => new[] { "EmployeeNumber", "PerformanceCycleNameAr", "PerformanceCycleNameEn", "PlanTitle", "SourceType", "TargetCompletionDate", "Notes" },
            "marketplace-opportunities" => new[] { "Title", "Description", "OpportunityType", "OrganizationUnitNameAr", "OrganizationUnitNameEn", "PositionTitleAr", "PositionTitleEn", "RequiredCompetencySummary", "OpenDate", "CloseDate", "MaxApplicants", "IsConfidential", "Notes" },
            _ => Array.Empty<string>()
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Columns().AdjustToContents();
    }

    private static void AddSampleRows(IXLWorksheet sheet, string table)
    {
        var rows = GetSampleRows(table);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            for (var j = 0; j < row.Length; j++)
            {
                sheet.Cell(i + 2, j + 1).Value = row[j];
            }
        }

        if (rows.Count > 0)
        {
            sheet.Range(2, 1, rows.Count + 1, rows[0].Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
        }

        sheet.Columns().AdjustToContents();
    }

    private static IReadOnlyList<string[]> GetSampleRows(string table) =>
        table switch
        {
            "organization-units" =>
            [
                ["القيادة التنفيذية", "Executive Leadership", "", ""],
                ["قطاع الخدمات المشتركة", "Shared Services Sector", "القيادة التنفيذية", "Executive Leadership"],
                ["مكتب الابتكار والتجربة", "Innovation & Experience Office", "قطاع الخدمات المشتركة", "Shared Services Sector"]
            ],
            "job-grades" =>
            [
                ["DEMO-SS-5", "5"],
                ["DEMO-SS-6", "6"]
            ],
            "positions" =>
            [
                ["قائد تجربة الموظف", "Employee Experience Lead", "مكتب الابتكار والتجربة", "Innovation & Experience Office", "DEMO-SS-5"],
                ["محلل بيانات أعمال", "Business Data Analyst", "قطاع الخدمات المشتركة", "Shared Services Sector", "DEMO-SS-6"]
            ],
            "competency-categories" =>
            [
                ["كفاءات عرض توضيحية — سلوكية", "Demo — Behavioral", "أمثلة لسلوكيات وظيفية"],
                ["كفاءات عرض توضيحية — تحليل", "Demo — Analytics", "أمثلة لمهارات تحليل البيانات"]
            ],
            "competency-levels" =>
            [
                ["مستوى عرض توضيحي — أساسي", "81", "مناسب لاختبار الاستيراد (قيمة رقمية نادرة)"],
                ["مستوى عرض توضيحي — متقدم", "82", "مناسب لاختبار الاستيراد (قيمة رقمية نادرة)"]
            ],
            "competencies" =>
            [
                ["DEMO-COM-01", "التواصل التشاركي", "Collaborative Communication", "أسلوب عمل جماعي فعال", "كفاءات عرض توضيحية — سلوكية", "Demo — Behavioral"],
                ["DEMO-ANA-02", "تحليل البيانات الوصفية", "Descriptive Data Analysis", "قراءة مؤشرات وتقارير", "كفاءات عرض توضيحية — تحليل", "Demo — Analytics"]
            ],
            "employees" =>
            [
                ["DEMO-EMP-501", "خالد إبراهيم", "Khalid Ibrahim", "khalid.ibrahim@talentos.local", "مكتب الابتكار والتجربة", "Innovation & Experience Office", "قائد تجربة الموظف", "Employee Experience Lead"],
                ["DEMO-EMP-502", "ليلى حسن", "Layla Hassan", "layla.hassan@talentos.local", "قطاع الخدمات المشتركة", "Shared Services Sector", "محلل بيانات أعمال", "Business Data Analyst"]
            ],
            "roles" =>
            [
                ["مسؤول عرض توضيحي — إدارة", "Demo — Admin", "وصول إداري للاختبار", "Demo administrative access", "false"],
                ["مسؤول عرض توضيحي — موارد بشرية", "Demo — HR", "وصول موارد بشرية للاختبار", "Demo HR access", "false"]
            ],
            "users" =>
            [
                ["demo.admin.khalid", "خالد إبراهيم", "Khalid Ibrahim", "khalid.ibrahim@talentos.local", "P@ssw0rd123", "DEMO-EMP-501", "مسؤول عرض توضيحي — إدارة"],
                ["demo.hr.layla", "ليلى حسن", "Layla Hassan", "layla.hassan@talentos.local", "P@ssw0rd123", "DEMO-EMP-502", "مسؤول عرض توضيحي — موارد بشرية"]
            ],
            "performance-cycles" =>
            [
                ["دورة أداء عرض 2026", "Demo Performance Cycle 2026", "2026-01-01", "2026-12-31", "دورة كاملة تغطي عام 2026 للاختبار"],
                ["دورة أداء عرض 2027", "Demo Performance Cycle 2027", "2027-01-01", "2027-12-31", "دورة ثانية للاختبار والخطط"]
            ],
            "critical-positions" =>
            [
                ["قائد تجربة الموظف", "Employee Experience Lead", "High", "Medium", "منصب حساس على تجربة الموظفين"],
                ["محلل بيانات أعمال", "Business Data Analyst", "Medium", "Low", "منصب داعم لاتخاذ القرار"]
            ],
            "job-competency-requirements" =>
            [
                ["قائد تجربة الموظف", "Employee Experience Lead", "DEMO-COM-01", "مستوى عرض توضيحي — متقدم"],
                ["محلل بيانات أعمال", "Business Data Analyst", "DEMO-ANA-02", "مستوى عرض توضيحي — متقدم"]
            ],
            "succession-plans" =>
            [
                ["خطة تعاقب — تجربة الموظف 2026", "قائد تجربة الموظف", "Employee Experience Lead", "دورة أداء عرض 2026", "Demo Performance Cycle 2026", "خطط بديلة لمنصب التجربة"],
                ["خطة تعاقب — تحليل البيانات 2027", "محلل بيانات أعمال", "Business Data Analyst", "دورة أداء عرض 2027", "Demo Performance Cycle 2027", "خطط بديلة لمنصب التحليل"]
            ],
            "development-plans" =>
            [
                ["DEMO-EMP-501", "دورة أداء عرض 2026", "Demo Performance Cycle 2026", "خطة تطوير — خالد", "Performance", "2026-11-30", "تطوير أداء ومهارات قيادية"],
                ["DEMO-EMP-502", "دورة أداء عرض 2027", "Demo Performance Cycle 2027", "خطة تطوير — ليلى", "CompetencyGap", "2027-09-30", "سد فجوة تحليل البيانات"]
            ],
            "marketplace-opportunities" =>
            [
                ["فرصة عرض — مشروع تجربة الموظف", "مشروع داخلي قصير لتحسين رحلة الموظف", "Project", "مكتب الابتكار والتجربة", "Innovation & Experience Office", "قائد تجربة الموظف", "Employee Experience Lead", "تواصل وتجربة مستخدم", "2026-01-15", "2026-12-31", "12", "false", "فرصة للعرض التجريبي"],
                ["فرصة عرض — ورشة تحليل البيانات", "ورشة داخلية لرفع مهارات التحليل", "TaskForce", "قطاع الخدمات المشتركة", "Shared Services Sector", "محلل بيانات أعمال", "Business Data Analyst", "تحليل واتصال بصري بالبيانات", "2026-02-01", "2026-12-31", "15", "false", "فرصة ثانية للعرض"]
            ],
            _ => Array.Empty<string[]>()
        };

    private static Dictionary<string, int> ReadHeaders(IXLWorksheet sheet)
    {
        var usedRange = sheet.RangeUsed();
        if (usedRange is null)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        var headerRow = usedRange.FirstRow();
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headerMap[header] = cell.Address.ColumnNumber;
            }
        }

        return headerMap;
    }

    private static IEnumerable<IXLRangeRow> ReadDataRows(IXLWorksheet sheet)
    {
        var usedRange = sheet.RangeUsed();
        if (usedRange is null)
        {
            return Enumerable.Empty<IXLRangeRow>();
        }

        return usedRange.RowsUsed().Where(r => r.RowNumber() > usedRange.FirstRow().RowNumber());
    }

    private static string GetString(IXLRangeRow row, IReadOnlyDictionary<string, int> headers, string name)
    {
        if (!headers.TryGetValue(name, out var index))
        {
            return string.Empty;
        }

        return row.Cell(index).GetString().Trim();
    }

    private static bool TryResolveReferenceId(
        string? nameAr,
        string? nameEn,
        IReadOnlyDictionary<string, Guid> byNameAr,
        IReadOnlyDictionary<string, Guid> byNameEn,
        out Guid id)
    {
        if (TryGetFromLookup(byNameAr, nameAr, out id))
        {
            return true;
        }

        if (TryGetFromLookup(byNameEn, nameEn, out id))
        {
            return true;
        }

        return false;
    }

    private static bool TryGetFromLookup(IReadOnlyDictionary<string, Guid> map, string? rawKey, out Guid id)
    {
        id = Guid.Empty;
        var normalized = NormalizeLookupKey(rawKey);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        if (map.TryGetValue(normalized, out id))
        {
            return true;
        }

        foreach (var item in map)
        {
            if (NormalizeLookupKey(item.Key) == normalized)
            {
                id = item.Value;
                return true;
            }
        }

        return false;
    }

    private static bool ContainsLookup(IReadOnlyDictionary<string, Guid> map, string? rawKey) =>
        TryGetFromLookup(map, rawKey, out _);

    private static void AddOrUpdateLookup(IDictionary<string, Guid> map, string rawKey, Guid id)
    {
        var normalized = NormalizeLookupKey(rawKey);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        map[normalized] = id;
    }

    private static string NormalizeLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compact = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return compact.ToLowerInvariant();
    }

    private static bool TryParseEnum<TEnum>(string value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out parsed);
    }

    private async Task<Dictionary<string, Guid>> LoadOrganizationUnitsByNameAr(CancellationToken cancellationToken)
    {
        var data = await LoadAllOrganizationUnits(cancellationToken);
        return data.Where(x => !string.IsNullOrWhiteSpace(x.NameAr))
            .ToDictionary(x => x.NameAr.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Guid>> LoadOrganizationUnitsByNameEn(CancellationToken cancellationToken)
    {
        var data = await LoadAllOrganizationUnits(cancellationToken);
        return data.Where(x => !string.IsNullOrWhiteSpace(x.NameEn))
            .ToDictionary(x => x.NameEn.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<OrganizationUnitDto>> LoadAllOrganizationUnits(CancellationToken cancellationToken)
    {
        var page = 1;
        var items = new List<OrganizationUnitDto>();
        while (true)
        {
            var result = await _organizationUnitService.GetPagedAsync(new OrganizationUnitFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            items.AddRange(result.Value!.Items);
            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private async Task<Dictionary<string, Guid>> LoadJobGradesByName(CancellationToken cancellationToken)
    {
        var page = 1;
        var map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _jobGradeService.GetPagedAsync(new JobGradeFilterRequest { Page = page, PageSize = 200 }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
            {
                map[item.Name.Trim()] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return map;
    }

    private async Task<HashSet<string>> LoadPositionsByTitleAr(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _positionService.GetPagedAsync(new PositionFilterRequest { Page = page, PageSize = 200 }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.TitleAr)))
            {
                values.Add(item.TitleAr.Trim());
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadPositionsByTitleArMap(CancellationToken cancellationToken)
    {
        var items = await LoadAllPositions(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.TitleAr))
            .ToDictionary(x => x.TitleAr.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Guid>> LoadPositionsByTitleEnMap(CancellationToken cancellationToken)
    {
        var items = await LoadAllPositions(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.TitleEn))
            .ToDictionary(x => x.TitleEn.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<PositionDto>> LoadAllPositions(CancellationToken cancellationToken)
    {
        var page = 1;
        var items = new List<PositionDto>();
        while (true)
        {
            var result = await _positionService.GetPagedAsync(new PositionFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            items.AddRange(result.Value!.Items);
            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private async Task<Dictionary<string, Guid>> LoadCompetencyCategoriesByNameAr(CancellationToken cancellationToken)
    {
        var items = await LoadAllCompetencyCategories(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.NameAr))
            .ToDictionary(x => x.NameAr.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Guid>> LoadCompetencyCategoriesByNameEn(CancellationToken cancellationToken)
    {
        var items = await LoadAllCompetencyCategories(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.NameEn))
            .ToDictionary(x => x.NameEn.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<CompetencyCategoryDto>> LoadAllCompetencyCategories(CancellationToken cancellationToken)
    {
        var page = 1;
        var items = new List<CompetencyCategoryDto>();
        while (true)
        {
            var result = await _competencyCategoryService.GetPagedAsync(new CompetencyCategoryFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            items.AddRange(result.Value!.Items);
            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private async Task<HashSet<string>> LoadCompetencyLevelNames(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _competencyLevelService.GetPagedAsync(new CompetencyLevelFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
            {
                values.Add(item.Name.Trim());
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<HashSet<string>> LoadCompetencyCodes(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _competencyService.GetPagedAsync(new CompetencyFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Code)))
            {
                values.Add(item.Code.Trim());
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadEmployeesByNumber(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _employeeService.GetPagedAsync(new EmployeeFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeNumber)))
            {
                values[item.EmployeeNumber.Trim()] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadRolesByNameAr(CancellationToken cancellationToken)
    {
        var roles = await LoadAllRoles(cancellationToken);
        return roles.Where(x => !string.IsNullOrWhiteSpace(x.NameAr))
            .ToDictionary(x => x.NameAr.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Guid>> LoadRolesByNameEn(CancellationToken cancellationToken)
    {
        var roles = await LoadAllRoles(cancellationToken);
        return roles.Where(x => !string.IsNullOrWhiteSpace(x.NameEn))
            .ToDictionary(x => x.NameEn.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<RoleListItemDto>> LoadAllRoles(CancellationToken cancellationToken)
    {
        var page = 1;
        var items = new List<RoleListItemDto>();
        while (true)
        {
            var result = await _roleService.GetPagedAsync(new RoleFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            items.AddRange(result.Value!.Items);
            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private async Task<HashSet<string>> LoadUsersByUserName(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _userService.GetPagedAsync(new UserFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.UserName)))
            {
                values.Add(item.UserName.Trim());
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<Guid, Guid>> LoadCriticalPositionByPositionId(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new Dictionary<Guid, Guid>();
        while (true)
        {
            var result = await _criticalPositionService.GetPagedAsync(new CriticalPositionFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items)
            {
                values[item.PositionId] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<HashSet<string>> LoadJobCompetencyRequirementKeys(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _jobCompetencyRequirementService.GetPagedAsync(new JobCompetencyRequirementFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items)
            {
                values.Add($"{item.PositionId}:{item.CompetencyId}:{item.RequiredLevelId}");
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<HashSet<string>> LoadSuccessionPlanKeys(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _successionPlanService.GetPagedAsync(new SuccessionPlanFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items)
            {
                values.Add($"{item.CriticalPositionId}:{item.PerformanceCycleId}:{item.PlanName}");
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<HashSet<string>> LoadDevelopmentPlanKeys(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _developmentPlanService.GetPagedAsync(new DevelopmentPlanFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items)
            {
                values.Add($"{item.EmployeeId}:{item.PerformanceCycleId}:{item.PlanTitle}");
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadOpportunitiesByTitle(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _marketplaceOpportunityService.GetPagedAsync(new MarketplaceOpportunityFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Title)))
            {
                values[item.Title.Trim()] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadPerformanceCyclesByNameAr(CancellationToken cancellationToken)
    {
        var items = await LoadAllPerformanceCycles(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.NameAr))
            .ToDictionary(x => x.NameAr.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, Guid>> LoadPerformanceCyclesByNameEn(CancellationToken cancellationToken)
    {
        var items = await LoadAllPerformanceCycles(cancellationToken);
        return items.Where(x => !string.IsNullOrWhiteSpace(x.NameEn))
            .ToDictionary(x => x.NameEn.Trim(), x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<PerformanceCycleDto>> LoadAllPerformanceCycles(CancellationToken cancellationToken)
    {
        var page = 1;
        var items = new List<PerformanceCycleDto>();
        while (true)
        {
            var result = await _performanceCycleService.GetPagedAsync(new PerformanceCycleFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            items.AddRange(result.Value!.Items);
            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return items;
    }

    private async Task<Dictionary<string, Guid>> LoadCompetenciesByCode(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _competencyService.GetPagedAsync(new CompetencyFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Code)))
            {
                values[item.Code.Trim()] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    private async Task<Dictionary<string, Guid>> LoadCompetencyLevelsByName(CancellationToken cancellationToken)
    {
        var page = 1;
        var values = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var result = await _competencyLevelService.GetPagedAsync(new CompetencyLevelFilterRequest
            {
                Page = page,
                PageSize = 200
            }, cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            foreach (var item in result.Value!.Items.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
            {
                values[item.Name.Trim()] = item.Id;
            }

            if (!result.Value.HasNextPage)
            {
                break;
            }

            page++;
        }

        return values;
    }

    public sealed record ExcelImportResponseDto(IReadOnlyList<ExcelImportTableResultDto> Tables);

    public sealed record ExcelImportTableResultDto(
        string Table,
        int Inserted,
        int Skipped,
        IReadOnlyList<string> Errors);

    public sealed class ImportExcelRequest
    {
        public IFormFile? File { get; set; }

        public string? Table { get; set; } = "all";
    }
}
