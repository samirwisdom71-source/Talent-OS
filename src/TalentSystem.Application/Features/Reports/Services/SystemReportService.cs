using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TalentSystem.Application.Features.Reports.DTOs;
using TalentSystem.Application.Features.Reports.Interfaces;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Reports.Services;

public sealed class SystemReportService : ISystemReportService
{
    private static readonly MethodInfo BuildSummaryMethod = typeof(SystemReportService)
        .GetMethod(nameof(BuildTableSummaryAsync), BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException($"Method '{nameof(BuildTableSummaryAsync)}' was not found.");
    private static readonly HashSet<string> ExcludedEntityNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "User",
        "UserRole",
        "Role",
        "Permission",
        "RolePermission",
        "Notification",
        "NotificationDispatchLog",
        "NotificationTemplate"
    };

    private readonly TalentDbContext _db;
    private readonly IValidator<SystemReportFilterRequest> _validator;

    public SystemReportService(
        TalentDbContext db,
        IValidator<SystemReportFilterRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<Result<SystemReportDto>> BuildAsync(
        SystemReportFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<SystemReportDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var tableEntityTypes = _db.Model.GetEntityTypes()
            .Where(static entity => entity.ClrType is not null && !entity.IsOwned())
            .Where(entity => !ExcludedEntityNames.Contains(entity.ClrType.Name))
            .OrderBy(static entity => entity.ClrType.Name)
            .ToArray();

        var lookupContext = await BuildLookupContextAsync(_db, cancellationToken);
        var summaries = new List<SystemReportTableSummaryDto>(tableEntityTypes.Length);
        foreach (var entityType in tableEntityTypes)
        {
            var summary = await BuildSummaryForEntityAsync(entityType, filter, lookupContext, cancellationToken);
            summaries.Add(summary);
        }

        var language = NormalizeLanguage(filter.Language);
        var totalRecords = summaries.Sum(static table => table.RecordsCount);
        var domainSummaries = BuildDomainSummaries(summaries, language);
        var dto = new SystemReportDto(
            DateTime.UtcNow,
            filter.FromUtc,
            filter.ToUtc,
            language,
            summaries.Count,
            totalRecords,
            domainSummaries,
            summaries);

        return Result<SystemReportDto>.Ok(dto);
    }

    private Task<SystemReportTableSummaryDto> BuildSummaryForEntityAsync(
        IEntityType entityType,
        SystemReportFilterRequest filter,
        ReportLookupContext lookupContext,
        CancellationToken cancellationToken)
    {
        var genericMethod = BuildSummaryMethod.MakeGenericMethod(entityType.ClrType);
        var task = (Task<SystemReportTableSummaryDto>)genericMethod.Invoke(
            null,
            new object[] { _db, entityType, filter, lookupContext, cancellationToken })!;

        return task;
    }

    private static async Task<SystemReportTableSummaryDto> BuildTableSummaryAsync<TEntity>(
        TalentDbContext db,
        IEntityType entityType,
        SystemReportFilterRequest filter,
        ReportLookupContext lookupContext,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var hasCreatedOnUtc = entityType.FindProperty("CreatedOnUtc") is not null;
        IQueryable<TEntity> query = db.Set<TEntity>().AsNoTracking();

        if (hasCreatedOnUtc && filter.FromUtc.HasValue)
        {
            var from = filter.FromUtc.Value;
            query = query.Where(entity => EF.Property<DateTime>(entity, "CreatedOnUtc") >= from);
        }

        if (hasCreatedOnUtc && filter.ToUtc.HasValue)
        {
            var to = filter.ToUtc.Value;
            query = query.Where(entity => EF.Property<DateTime>(entity, "CreatedOnUtc") <= to);
        }

        var recordsCount = await query.CountAsync(cancellationToken);
        if (!hasCreatedOnUtc)
        {
            var (previewColumns, previewRows) = await BuildPreviewAsync(query, lookupContext, cancellationToken);
            return new SystemReportTableSummaryDto(
                entityType.ClrType.Name,
                entityType.GetTableName() ?? entityType.ClrType.Name,
                recordsCount,
                previewColumns,
                previewRows,
                new[] { new SystemReportChartPointDto("total", recordsCount) });
        }

        var toUtc = filter.ToUtc ?? DateTime.UtcNow;
        var computedFromUtc = new DateTime(toUtc.Year, toUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(filter.ChartMonths - 1));
        var chartFromUtc = filter.FromUtc.HasValue && filter.FromUtc.Value > computedFromUtc
            ? filter.FromUtc.Value
            : computedFromUtc;

        var chartPoints = await query
            .Where(entity => EF.Property<DateTime>(entity, "CreatedOnUtc") >= chartFromUtc)
            .GroupBy(entity => new
            {
                Year = EF.Property<DateTime>(entity, "CreatedOnUtc").Year,
                Month = EF.Property<DateTime>(entity, "CreatedOnUtc").Month
            })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Count = group.Count()
            })
            .OrderBy(item => item.Year)
            .ThenBy(item => item.Month)
            .ToListAsync(cancellationToken);

        var labels = BuildMonthLabels(chartFromUtc, toUtc);
        var points = labels
            .Select(label =>
            {
                var hit = chartPoints.FirstOrDefault(item => $"{item.Year:D4}-{item.Month:D2}" == label);
                return new SystemReportChartPointDto(label, hit?.Count ?? 0);
            })
            .ToArray();
        var (columns, rows) = await BuildPreviewAsync(query, lookupContext, cancellationToken);

        return new SystemReportTableSummaryDto(
            entityType.ClrType.Name,
            entityType.GetTableName() ?? entityType.ClrType.Name,
            recordsCount,
            columns,
            rows,
            points);
    }

    private static IReadOnlyList<string> BuildMonthLabels(DateTime fromUtc, DateTime toUtc)
    {
        var fromMonth = new DateTime(fromUtc.Year, fromUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toMonth = new DateTime(toUtc.Year, toUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var months = new List<string>();
        for (var month = fromMonth; month <= toMonth; month = month.AddMonths(1))
        {
            months.Add($"{month:yyyy-MM}");
        }

        return months;
    }

    private static IReadOnlyList<SystemReportDomainSummaryDto> BuildDomainSummaries(
        IReadOnlyList<SystemReportTableSummaryDto> tables,
        string language)
    {
        var groups = new Dictionary<string, (string Ar, string En, HashSet<string> Entities)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Employees"] = ("الموظفون والهوية", "Employees and Identity", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Employee", "User", "Role", "Permission", "UserRole", "RolePermission"
            }),
            ["Jobs and Organization"] = ("الهيكل والوظائف", "Jobs and Organization", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "OrganizationUnit", "Position", "JobGrade", "JobCompetencyRequirement"
            }),
            ["Performance and Cycles"] = ("الأداء والدورات", "Performance and Cycles", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PerformanceCycle", "PerformanceGoal", "PerformanceEvaluation"
            }),
            ["Potential and Talent"] = ("الإمكانات والمواهب", "Potential and Talent", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PotentialAssessment", "PotentialAssessmentFactor", "TalentProfile", "TalentScore",
                "TalentClassification", "ClassificationRuleSet"
            }),
            ["Succession and Critical Roles"] = ("التعاقب والوظائف الحرجة", "Succession and Critical Roles", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CriticalPosition", "SuccessionPlan", "SuccessorCandidate", "SuccessionCoverageSnapshot"
            }),
            ["Development"] = ("التطوير", "Development", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DevelopmentPlan", "DevelopmentPlanItem", "DevelopmentPlanLink"
            }),
            ["Marketplace and Opportunities"] = ("السوق الداخلي والفرص", "Marketplace and Opportunities", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MarketplaceOpportunity", "OpportunityApplication", "OpportunityMatchSnapshot"
            }),
            ["Approvals and Notifications"] = ("الموافقات والإشعارات", "Approvals and Notifications", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ApprovalRequest", "ApprovalAction", "ApprovalAssignment",
                "Notification", "NotificationTemplate", "NotificationDispatchLog"
            }),
            ["Intelligence"] = ("الذكاء المؤسسي", "Intelligence", new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TalentInsight", "TalentRecommendation", "IntelligenceRun"
            })
        };

        var result = new List<SystemReportDomainSummaryDto>();
        var mappedEntities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups.Values)
        {
            var items = tables
                .Where(table => group.Entities.Contains(table.EntityName))
                .OrderByDescending(table => table.RecordsCount)
                .ThenBy(table => table.EntityName)
                .ToArray();

            if (items.Length == 0)
            {
                continue;
            }

            foreach (var item in items)
            {
                mappedEntities.Add(item.EntityName);
            }

            var domainChart = AggregateChartPoints(items);
            result.Add(new SystemReportDomainSummaryDto(
                language == "ar" ? group.Ar : group.En,
                items.Sum(static item => item.RecordsCount),
                domainChart,
                items));
        }

        var others = tables
            .Where(table => !mappedEntities.Contains(table.EntityName))
            .OrderByDescending(table => table.RecordsCount)
            .ThenBy(table => table.EntityName)
            .ToArray();

        if (others.Length > 0)
        {
            var othersChart = AggregateChartPoints(others);
            result.Add(new SystemReportDomainSummaryDto(
                language == "ar" ? "أخرى" : "Other",
                others.Sum(static item => item.RecordsCount),
                othersChart,
                others));
        }

        return result
            .OrderByDescending(domain => domain.TotalRecords)
            .ThenBy(domain => domain.DomainName)
            .ToArray();
    }

    private static IReadOnlyList<SystemReportChartPointDto> AggregateChartPoints(
        IReadOnlyList<SystemReportTableSummaryDto> tables)
    {
        var grouped = tables
            .SelectMany(table => table.ChartPoints)
            .GroupBy(point => point.Label, StringComparer.OrdinalIgnoreCase)
            .Select(group => new SystemReportChartPointDto(
                group.Key,
                group.Sum(point => point.Value)))
            .OrderBy(point => point.Label, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return grouped;
    }

    private static string NormalizeLanguage(string? language)
    {
        return string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase)
            ? "ar"
            : "en";
    }

    private static async Task<(IReadOnlyList<string> Columns, IReadOnlyList<SystemReportTableRowDto> Rows)> BuildPreviewAsync<TEntity>(
        IQueryable<TEntity> query,
        ReportLookupContext lookupContext,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var properties = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static prop => prop.GetMethod is not null && IsPreviewType(prop.PropertyType))
            .Where(static prop => !IsHiddenPreviewProperty(prop.Name))
            .OrderBy(static prop => prop.Name, StringComparer.Ordinal)
            .Take(8)
            .ToArray();

        var columns = properties.Select(static prop => prop.Name).ToArray();
        if (columns.Length == 0)
        {
            return (Array.Empty<string>(), Array.Empty<SystemReportTableRowDto>());
        }

        var entities = await query
            .Take(15)
            .ToListAsync(cancellationToken);

        var rows = entities
            .Select(entity =>
            {
                var cells = properties
                    .Select(prop => FormatPreviewValue(prop.Name, prop.GetValue(entity), lookupContext))
                    .ToArray();
                return new SystemReportTableRowDto(cells);
            })
            .ToArray();

        return (columns, rows);
    }

    private static bool IsPreviewType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying.IsEnum)
        {
            return true;
        }

        return underlying == typeof(string) ||
               underlying == typeof(Guid) ||
               underlying == typeof(int) ||
               underlying == typeof(long) ||
               underlying == typeof(short) ||
               underlying == typeof(decimal) ||
               underlying == typeof(double) ||
               underlying == typeof(float) ||
               underlying == typeof(bool) ||
               underlying == typeof(DateTime) ||
               underlying == typeof(DateOnly) ||
               underlying == typeof(TimeOnly);
    }

    private static bool IsHiddenPreviewProperty(string propertyName)
    {
        if (propertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
            propertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatPreviewValue(
        string propertyName,
        object? value,
        ReportLookupContext lookupContext)
    {
        if (value is null)
        {
            return "-";
        }

        if (value is Guid guidValue)
        {
            if (lookupContext.TryResolve(propertyName, guidValue, out var friendly))
            {
                return friendly;
            }

            return ShortGuid(guidValue);
        }

        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            TimeOnly t => t.ToString("HH:mm:ss"),
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? "-"
        };
    }

    private static string ShortGuid(Guid value)
    {
        var text = value.ToString();
        return text.Length <= 8 ? text : text[..8];
    }

    private static async Task<ReportLookupContext> BuildLookupContextAsync(
        TalentDbContext db,
        CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking()
            .Select(user => new { user.Id, user.UserName, user.NameEn, user.NameAr })
            .ToDictionaryAsync(
                item => item.Id,
                item =>
                {
                    if (!string.IsNullOrWhiteSpace(item.NameEn))
                    {
                        return item.NameEn;
                    }

                    if (!string.IsNullOrWhiteSpace(item.NameAr))
                    {
                        return item.NameAr;
                    }

                    return item.UserName;
                },
                cancellationToken);

        var employees = await db.Employees.AsNoTracking()
            .Select(employee => new { employee.Id, employee.FullNameEn, employee.FullNameAr, employee.EmployeeNumber })
            .ToDictionaryAsync(
                item => item.Id,
                item =>
                {
                    var name = !string.IsNullOrWhiteSpace(item.FullNameEn)
                        ? item.FullNameEn
                        : item.FullNameAr;
                    return $"{name} [{item.EmployeeNumber}]";
                },
                cancellationToken);

        var roles = await db.Roles.AsNoTracking()
            .Select(role => new { role.Id, role.NameEn, role.NameAr })
            .ToDictionaryAsync(
                item => item.Id,
                item => !string.IsNullOrWhiteSpace(item.NameEn) ? item.NameEn : item.NameAr ?? "-",
                cancellationToken);

        var units = await db.OrganizationUnits.AsNoTracking()
            .Select(unit => new { unit.Id, unit.NameEn, unit.NameAr })
            .ToDictionaryAsync(
                item => item.Id,
                item => !string.IsNullOrWhiteSpace(item.NameEn) ? item.NameEn : item.NameAr ?? "-",
                cancellationToken);

        var positions = await db.Positions.AsNoTracking()
            .Select(position => new { position.Id, position.TitleEn, position.TitleAr })
            .ToDictionaryAsync(
                item => item.Id,
                item => !string.IsNullOrWhiteSpace(item.TitleEn) ? item.TitleEn : item.TitleAr ?? "-",
                cancellationToken);

        var cycles = await db.PerformanceCycles.AsNoTracking()
            .Select(cycle => new { cycle.Id, cycle.NameEn, cycle.NameAr })
            .ToDictionaryAsync(
                item => item.Id,
                item => !string.IsNullOrWhiteSpace(item.NameEn) ? item.NameEn : item.NameAr ?? "-",
                cancellationToken);

        return new ReportLookupContext(users, employees, roles, units, positions, cycles);
    }

    private sealed class ReportLookupContext
    {
        private readonly IReadOnlyDictionary<Guid, string> _users;
        private readonly IReadOnlyDictionary<Guid, string> _employees;
        private readonly IReadOnlyDictionary<Guid, string> _roles;
        private readonly IReadOnlyDictionary<Guid, string> _units;
        private readonly IReadOnlyDictionary<Guid, string> _positions;
        private readonly IReadOnlyDictionary<Guid, string> _cycles;

        public ReportLookupContext(
            IReadOnlyDictionary<Guid, string> users,
            IReadOnlyDictionary<Guid, string> employees,
            IReadOnlyDictionary<Guid, string> roles,
            IReadOnlyDictionary<Guid, string> units,
            IReadOnlyDictionary<Guid, string> positions,
            IReadOnlyDictionary<Guid, string> cycles)
        {
            _users = users;
            _employees = employees;
            _roles = roles;
            _units = units;
            _positions = positions;
            _cycles = cycles;
        }

        public bool TryResolve(string propertyName, Guid value, out string resolved)
        {
            var key = propertyName.Trim();
            if (key.EndsWith("UserId", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("CreatedByUserId", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("ModifiedByUserId", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("DeletedByUserId", StringComparison.OrdinalIgnoreCase))
            {
                if (_users.TryGetValue(value, out var userName))
                {
                    resolved = userName;
                    return true;
                }
            }

            if (key.Equals("EmployeeId", StringComparison.OrdinalIgnoreCase) && _employees.TryGetValue(value, out var employeeName))
            {
                resolved = employeeName;
                return true;
            }

            if (key.Equals("RoleId", StringComparison.OrdinalIgnoreCase) && _roles.TryGetValue(value, out var roleName))
            {
                resolved = roleName;
                return true;
            }

            if (key.Equals("OrganizationUnitId", StringComparison.OrdinalIgnoreCase) && _units.TryGetValue(value, out var unitName))
            {
                resolved = unitName;
                return true;
            }

            if (key.Equals("PositionId", StringComparison.OrdinalIgnoreCase) && _positions.TryGetValue(value, out var positionName))
            {
                resolved = positionName;
                return true;
            }

            if (key.Equals("PerformanceCycleId", StringComparison.OrdinalIgnoreCase) && _cycles.TryGetValue(value, out var cycleName))
            {
                resolved = cycleName;
                return true;
            }

            resolved = string.Empty;
            return false;
        }
    }
}
