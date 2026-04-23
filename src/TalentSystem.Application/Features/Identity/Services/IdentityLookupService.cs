using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Domain.Identity;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Services;

public sealed class IdentityLookupService : IIdentityLookupService
{
    private const int MaxTake = 200;
    private readonly TalentDbContext _db;

    public IdentityLookupService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetEmployeesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullNameAr)
            .ThenBy(e => e.FullNameEn)
            .Select(e => new LookupItemDto
            {
                Id = e.Id,
                Name = string.IsNullOrWhiteSpace(e.FullNameAr)
                    ? e.FullNameEn
                    : e.FullNameAr,
                Email = e.Email
            });

        var employees = await ApplySearchAndTake(query, search, take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(employees);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetUsersAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        var preferEn = PreferEnglish(displayLang);
        IQueryable<User> baseQuery = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(u =>
                u.UserName.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.NameAr != null && u.NameAr.Contains(term)) ||
                (u.NameEn != null && u.NameEn.Contains(term)));
        }

        IQueryable<User> ordered = baseQuery.OrderBy(u => u.UserName);
        if (take.HasValue && take.Value > 0)
        {
            ordered = ordered.Take(Math.Min(take.Value, MaxTake));
        }

        var rows = await ordered
            .Select(u => new { u.Id, u.NameAr, u.NameEn, u.UserName, u.Email })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var users = rows
            .Select(u => new LookupItemDto
            {
                Id = u.Id,
                Name = PickBilingualPersonName(u.NameAr, u.NameEn, u.UserName, preferEn),
                Email = u.Email,
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(users);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetRolesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.NameAr)
            .ThenBy(r => r.NameEn)
            .Select(r => new LookupItemDto
            {
                Id = r.Id,
                Name = string.IsNullOrWhiteSpace(r.NameAr) ? r.NameEn : r.NameAr
            });

        var roles = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(roles);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetPermissionsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.NameAr)
            .ThenBy(p => p.NameEn)
            .Select(p => new LookupItemDto
            {
                Id = p.Id,
                Name = string.IsNullOrWhiteSpace(p.NameAr) ? p.NameEn : p.NameAr
            });

        var permissions = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(permissions);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetPositionsAsync(
        string? search = null,
        int? take = null,
        Guid? organizationUnitId = null,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _db.Positions.AsNoTracking().AsQueryable();

        if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(p => p.OrganizationUnitId == organizationUnitId.Value);
        }

        var query = baseQuery
            .OrderBy(p => p.TitleAr)
            .ThenBy(p => p.TitleEn)
            .Select(p => new LookupItemDto
            {
                Id = p.Id,
                Name = string.IsNullOrWhiteSpace(p.TitleAr)
                    ? p.TitleEn
                    : p.TitleAr
            });

        var positions = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(positions);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetOrganizationUnitsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.OrganizationUnits
            .AsNoTracking()
            .OrderBy(o => o.NameAr)
            .ThenBy(o => o.NameEn)
            .Select(o => new LookupItemDto
            {
                Id = o.Id,
                Name = string.IsNullOrWhiteSpace(o.NameAr)
                    ? o.NameEn
                    : o.NameAr
            });

        var units = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(units);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetJobGradesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.JobGrades
            .AsNoTracking()
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name)
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = $"{x.Name} (L{x.Level})"
            });

        var items = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetCompetenciesAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Competencies
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ThenBy(x => x.NameEn)
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = string.IsNullOrWhiteSpace(x.NameAr)
                    ? x.NameEn
                    : x.NameAr
            });

        var items = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetCompetencyLevelsAsync(
        string? search = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.CompetencyLevels
            .AsNoTracking()
            .OrderBy(x => x.NumericValue)
            .ThenBy(x => x.Name)
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = $"{x.Name} (L{x.NumericValue})"
            });

        var items = await ApplySearchAndTake(query, search, take, includeEmailSearch: false)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetPerformanceEvaluationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        var preferEn = PreferEnglish(displayLang);
        var baseQuery =
            from ev in _db.PerformanceEvaluations.AsNoTracking()
            join emp in _db.Employees.AsNoTracking() on ev.EmployeeId equals emp.Id
            orderby ev.EvaluatedOnUtc descending, emp.FullNameAr
            select new
            {
                ev.Id,
                EmpNameAr = emp.FullNameAr,
                EmpNameEn = emp.FullNameEn,
                ev.OverallScore,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(x =>
                (x.EmpNameAr != null && x.EmpNameAr.Contains(term)) ||
                (x.EmpNameEn != null && x.EmpNameEn.Contains(term)));
        }

        if (take.HasValue && take.Value > 0)
        {
            baseQuery = baseQuery.Take(Math.Min(take.Value, MaxTake));
        }

        var rows = await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var items = rows
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = PickBilingualPersonName(
                    x.EmpNameAr,
                    x.EmpNameEn,
                    x.EmpNameAr ?? x.EmpNameEn ?? string.Empty,
                    preferEn),
                Email = preferEn
                    ? $"Score: {x.OverallScore}"
                    : $"الدرجة: {x.OverallScore}",
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetTalentClassificationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        var preferEn = PreferEnglish(displayLang);
        var baseQuery =
            from tc in _db.TalentClassifications.AsNoTracking()
            join emp in _db.Employees.AsNoTracking() on tc.EmployeeId equals emp.Id
            orderby tc.ClassifiedOnUtc descending, emp.FullNameAr
            select new
            {
                tc.Id,
                EmpNameAr = emp.FullNameAr,
                EmpNameEn = emp.FullNameEn,
                tc.CategoryName,
                tc.NineBoxCode,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(x =>
                (x.EmpNameAr != null && x.EmpNameAr.Contains(term)) ||
                (x.EmpNameEn != null && x.EmpNameEn.Contains(term)) ||
                (x.CategoryName != null && x.CategoryName.Contains(term)));
        }

        if (take.HasValue && take.Value > 0)
        {
            baseQuery = baseQuery.Take(Math.Min(take.Value, MaxTake));
        }

        var rows = await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var items = rows
            .Select(x =>
            {
                var sub = string.IsNullOrWhiteSpace(x.CategoryName)
                    ? $"9-Box {(int)x.NineBoxCode}"
                    : x.CategoryName;
                return new LookupItemDto
                {
                    Id = x.Id,
                    Name = PickBilingualPersonName(
                        x.EmpNameAr,
                        x.EmpNameEn,
                        x.EmpNameAr ?? x.EmpNameEn ?? string.Empty,
                        preferEn),
                    Email = sub,
                };
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetDevelopmentPlansAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        var preferEn = PreferEnglish(displayLang);
        var baseQuery =
            from dp in _db.DevelopmentPlans.AsNoTracking()
            join emp in _db.Employees.AsNoTracking() on dp.EmployeeId equals emp.Id
            orderby dp.PlanTitle
            select new
            {
                dp.Id,
                dp.PlanTitle,
                EmpNameAr = emp.FullNameAr,
                EmpNameEn = emp.FullNameEn,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(x =>
                (x.PlanTitle != null && x.PlanTitle.Contains(term)) ||
                (x.EmpNameAr != null && x.EmpNameAr.Contains(term)) ||
                (x.EmpNameEn != null && x.EmpNameEn.Contains(term)));
        }

        if (take.HasValue && take.Value > 0)
        {
            baseQuery = baseQuery.Take(Math.Min(take.Value, MaxTake));
        }

        var rows = await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var items = rows
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.PlanTitle,
                Email = PickBilingualPersonName(
                    x.EmpNameAr,
                    x.EmpNameEn,
                    x.EmpNameAr ?? x.EmpNameEn ?? string.Empty,
                    preferEn),
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetMarketplaceOpportunitiesAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        _ = displayLang; // Titles are single-locale; reserved for future use.
        var query = _db.MarketplaceOpportunities
            .AsNoTracking()
            .OrderByDescending(x => x.OpenDate)
            .ThenBy(x => x.Title)
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.Title,
                Email = x.Description,
            });

        var items = await ApplySearchAndTake(query, search, take, includeEmailSearch: true)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    public async Task<Result<IReadOnlyList<LookupItemDto>>> GetOpportunityApplicationsAsync(
        string? search = null,
        int? take = null,
        string? displayLang = null,
        CancellationToken cancellationToken = default)
    {
        var preferEn = PreferEnglish(displayLang);
        var baseQuery =
            from app in _db.OpportunityApplications.AsNoTracking()
            join emp in _db.Employees.AsNoTracking() on app.EmployeeId equals emp.Id
            join opp in _db.MarketplaceOpportunities.AsNoTracking() on app.MarketplaceOpportunityId equals opp.Id
            orderby app.AppliedOnUtc descending
            select new
            {
                app.Id,
                EmpNameAr = emp.FullNameAr,
                EmpNameEn = emp.FullNameEn,
                OppTitle = opp.Title,
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(x =>
                (x.EmpNameAr != null && x.EmpNameAr.Contains(term)) ||
                (x.EmpNameEn != null && x.EmpNameEn.Contains(term)) ||
                (x.OppTitle != null && x.OppTitle.Contains(term)));
        }

        if (take.HasValue && take.Value > 0)
        {
            baseQuery = baseQuery.Take(Math.Min(take.Value, MaxTake));
        }

        var rows = await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var items = rows
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = PickBilingualPersonName(
                    x.EmpNameAr,
                    x.EmpNameEn,
                    x.EmpNameAr ?? x.EmpNameEn ?? string.Empty,
                    preferEn),
                Email = x.OppTitle,
            })
            .ToList();

        return Result<IReadOnlyList<LookupItemDto>>.Ok(items);
    }

    private static bool PreferEnglish(string? displayLang) =>
        string.Equals(displayLang, "en", StringComparison.OrdinalIgnoreCase);

    private static string PickBilingualPersonName(string? nameAr, string? nameEn, string fallback, bool preferEnglish)
    {
        if (preferEnglish)
        {
            if (!string.IsNullOrWhiteSpace(nameEn)) return nameEn!;
            if (!string.IsNullOrWhiteSpace(nameAr)) return nameAr!;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(nameAr)) return nameAr!;
            if (!string.IsNullOrWhiteSpace(nameEn)) return nameEn!;
        }

        return string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback;
    }

    private static IQueryable<LookupItemDto> ApplySearchAndTake(
        IQueryable<LookupItemDto> query,
        string? search,
        int? take,
        bool includeEmailSearch = true)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (includeEmailSearch)
            {
                query = query.Where(x =>
                    x.Name.Contains(term) ||
                    (x.Email != null && x.Email.Contains(term)));
            }
            else
            {
                query = query.Where(x => x.Name.Contains(term));
            }
        }

        if (take.HasValue && take.Value > 0)
        {
            query = query.Take(Math.Min(take.Value, MaxTake));
        }

        return query;
    }
}
