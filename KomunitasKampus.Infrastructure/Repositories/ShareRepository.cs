using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class ShareRepository : IShareRepository
{
    private readonly AppDbContext _context;

    public ShareRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Share share,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Shares.AddAsync(share, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetShareCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Shares
            .CountAsync(
                share => share.PostId == postId,
                cancellationToken
            );
    }
}
