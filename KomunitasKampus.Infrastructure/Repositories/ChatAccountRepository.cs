using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class ChatAccountRepository : IChatAccountRepository
{
    private readonly AppDbContext _context;

    public ChatAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Accounts
            .Include(account => account.Organization)
            .FirstOrDefaultAsync(
                account => account.Id == accountId,
                cancellationToken
            );
    }
}
