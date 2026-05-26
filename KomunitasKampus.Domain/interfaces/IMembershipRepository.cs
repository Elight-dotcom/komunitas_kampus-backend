using System.Threading;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Interfaces;

public interface IMembershipRepository
{
    Task<Membership?> GetByIdAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Membership>> GetPendingRequestsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Membership>> GetMemberListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<Membership?> GetByAccountAndOrgAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<Membership?> GetByIdWithOrganizationAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default
    );

    Task CreateAsync(
        Membership membership,
        CancellationToken cancellationToken = default
    );

    Task UpdateStatusAsync(
        Guid membershipId,
        MembershipStatus status,
        DateTime? resolvedAt,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasActiveMembershipAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsInCooldownAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Membership>> GetInvitationsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Membership>> GetSentInvitationsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    );
}
