using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
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
            .Include(account => account.User)
            .FirstOrDefaultAsync(
                account => account.Id == accountId,
                cancellationToken
            );
    }

    public async Task<Account?> GetByEmailOrUsernameAsync(
        string emailOrUsername,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedIdentifier = emailOrUsername.Trim().ToLowerInvariant();

        return await _context.Accounts
            .Include(account => account.Organization)
            .Include(account => account.User)
            .FirstOrDefaultAsync(
                account =>
                    account.Email.ToLower() == normalizedIdentifier ||
                    account.Username.ToLower() == normalizedIdentifier,
                cancellationToken
            );
    }

    public async Task<bool> IsEmailTakenAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Accounts
            .AnyAsync(
                account => account.Email.ToLower() == normalizedEmail,
                cancellationToken
            );
    }

    public async Task<bool> IsUsernameTakenAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        return await _context.Accounts
            .AnyAsync(
                account => account.Username.ToLower() == normalizedUsername,
                cancellationToken
            );
    }

    public async Task<bool> IsOrganizationNameTakenAsync(
        string organizationName,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedOrganizationName = organizationName.Trim().ToLowerInvariant();

        return await _context.Organizations
            .AnyAsync(
                organization =>
                    organization.OrganizationName.ToLower() == normalizedOrganizationName,
                cancellationToken
            );
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}