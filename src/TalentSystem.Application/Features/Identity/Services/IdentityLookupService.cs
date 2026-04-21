using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Identity.Interfaces;
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
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            baseQuery = baseQuery.Where(u =>
                u.UserName.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.NameAr != null && u.NameAr.Contains(term)) ||
                (u.NameEn != null && u.NameEn.Contains(term)));
        }

        var query = baseQuery
            .OrderBy(u => u.UserName)
            .Select(u => new LookupItemDto
            {
                Id = u.Id,
                Name = string.IsNullOrWhiteSpace(u.NameAr)
                    ? (string.IsNullOrWhiteSpace(u.NameEn) ? u.UserName : u.NameEn)
                    : u.NameAr,
                Email = u.Email
            });

        var users = await ApplySearchAndTake(query, search: null, take, includeEmailSearch: true)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

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
        CancellationToken cancellationToken = default)
    {
        var query = _db.Positions
            .AsNoTracking()
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
