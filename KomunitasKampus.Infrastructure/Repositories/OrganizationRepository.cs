using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _context;

    public OrganizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Organization>> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var q = _context.Organizations
            .Include(o => o.Account)
            .Include(o => o.Memberships)
            .Include(o => o.Posts)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim().ToLower();
            q = q.Where(o =>
                o.OrganizationName.ToLower().Contains(normalized) ||
                o.Slug.ToLower().Contains(normalized) ||
                o.University.ToLower().Contains(normalized) ||
                (o.Description != null && o.Description.ToLower().Contains(normalized))
            );
        }

        return await q
            .OrderBy(o => o.OrganizationName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Organization>> GetRecommendedAsync(
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Organizations
            .Include(o => o.Account)
            .Include(o => o.Memberships)
            .Include(o => o.Posts)
            .AsNoTracking()
            .OrderBy(_ => Guid.NewGuid())
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Organizations
            .Include(o => o.Account)
            .Include(o => o.Memberships)
            .Include(o => o.Posts)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
