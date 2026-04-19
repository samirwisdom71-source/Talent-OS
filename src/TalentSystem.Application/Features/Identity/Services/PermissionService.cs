using Microsoft.EntityFrameworkCore;
using TalentSystem.Application.Features.Identity.DTOs;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Domain.Identity;
using TalentSystem.Persistence;
using TalentSystem.Shared.Results;

namespace TalentSystem.Application.Features.Identity.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly TalentDbContext _db;

    public PermissionService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Result> SeedDefaultPermissionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var def in PermissionDefinitions.All)
        {
            var exists = await _db.Permissions.AsNoTracking()
                .AnyAsync(p => p.Code == def.Code, cancellationToken)
                .ConfigureAwait(false);
            if (exists)
            {
                continue;
            }

            _db.Permissions.Add(new Permission
            {
                Code = def.Code,
                Name = def.Name,
                Module = def.Module
            });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Permissions.AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Module = p.Module
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<PermissionDto>>.Ok(list);
    }
}
