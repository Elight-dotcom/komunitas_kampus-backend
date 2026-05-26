using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMembershipStatus;

public class GetMembershipStatusQueryHandler
    : IRequestHandler<GetMembershipStatusQuery, MembershipStatusDto>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetMembershipStatusQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<MembershipStatusDto> Handle(
        GetMembershipStatusQuery request,
        CancellationToken cancellationToken
    )
    {
        var membership = await _membershipRepository.GetByAccountAndOrgAsync(
            request.AccountId,
            request.OrganizationId,
            cancellationToken
        );

        if (membership is null)
        {
            return new MembershipStatusDto(
                HasMembership: false,
                Status: null,
                InviteType: null,
                IsInCooldown: false,
                CanRequestAgainAt: null
            );
        }

        var isInCooldown = false;
        DateTime? canRequestAgainAt = null;

        if (membership.Status == MembershipStatus.Rejected && membership.ResolvedAt.HasValue)
        {
            canRequestAgainAt = membership.ResolvedAt.Value.AddDays(3);
            isInCooldown = DateTime.UtcNow < canRequestAgainAt.Value;
        }

        return new MembershipStatusDto(
            HasMembership: true,
            Status: membership.Status.ToString(),
            InviteType: membership.InviteType.ToString(),
            IsInCooldown: isInCooldown,
            CanRequestAgainAt: canRequestAgainAt
        );
    }
}
