using System.Threading;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly AppDbContext _context;

    public MembershipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Membership?> GetByIdAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Include(membership => membership.Organization)
                .ThenInclude(organization => organization!.Account)
            .FirstOrDefaultAsync(
                membership => membership.Id == membershipId,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Membership>> GetPendingRequestsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Include(membership => membership.Organization)
            .Where(membership =>
                membership.OrganizationId == organizationId &&
                membership.Status == MembershipStatus.Pending &&
                membership.InviteType == MembershipInviteType.Request
            )
            .OrderByDescending(membership => membership.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Membership>> GetMemberListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Include(membership => membership.Organization)
            .Where(membership =>
                membership.OrganizationId == organizationId &&
                membership.Status == MembershipStatus.Accepted
            )
            .OrderBy(membership => membership.Account!.User!.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Membership?> GetByAccountAndOrgAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Include(membership => membership.Organization)
            .Where(membership =>
                membership.AccountId == accountId &&
                membership.OrganizationId == organizationId
            )
            .OrderByDescending(membership => membership.RequestedAt)
            .ThenByDescending(membership => membership.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Membership?> GetByIdWithOrganizationAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .Include(membership => membership.Organization)
                .ThenInclude(organization => organization!.Account)
            .FirstOrDefaultAsync(
                membership => membership.Id == membershipId,
                cancellationToken
            );
    }

    public async Task CreateAsync(
        Membership membership,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Memberships.AddAsync(membership, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        Guid membershipId,
        MembershipStatus status,
        DateTime? resolvedAt,
        CancellationToken cancellationToken = default
    )
    {
        var membership = await _context.Memberships
            .FirstOrDefaultAsync(
                item => item.Id == membershipId,
                cancellationToken
            );

        if (membership is null)
        {
            return;
        }

        membership.Status = status;
        membership.ResolvedAt = resolvedAt;
        membership.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasActiveMembershipAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AnyAsync(
                membership =>
                    membership.AccountId == accountId &&
                    membership.OrganizationId == organizationId &&
                    membership.Status == MembershipStatus.Accepted,
                cancellationToken
            );
    }

    public async Task<bool> IsInCooldownAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        var cooldownStart = DateTime.UtcNow.AddDays(-3);

        return await _context.Memberships
            .AnyAsync(
                membership =>
                    membership.AccountId == accountId &&
                    membership.OrganizationId == organizationId &&
                    membership.Status == MembershipStatus.Rejected &&
                    membership.ResolvedAt != null &&
                    membership.ResolvedAt > cooldownStart,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Membership>> GetInvitationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(membership => membership.Organization)
                .ThenInclude(organization => organization!.Account)
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Where(membership =>
                membership.AccountId == accountId &&
                membership.InviteType == MembershipInviteType.Invite &&
                membership.Status == MembershipStatus.Pending
            )
            .OrderByDescending(membership => membership.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Membership>> GetSentInvitationsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AsNoTracking()
            .Include(membership => membership.Account)
                .ThenInclude(account => account!.User)
            .Include(membership => membership.Organization)
            .Where(membership =>
                membership.OrganizationId == organizationId &&
                membership.InviteType == MembershipInviteType.Invite
            )
            .OrderByDescending(membership => membership.RequestedAt)
            .ToListAsync(cancellationToken);
    }
}
