using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetInvitations;

public class GetInvitationsQueryHandler
    : IRequestHandler<GetInvitationsQuery, IReadOnlyList<InviteDto>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetInvitationsQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<IReadOnlyList<InviteDto>> Handle(
        GetInvitationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var invitations = await _membershipRepository.GetInvitationsAsync(
            request.AccountId,
            cancellationToken
        );

        return invitations
            .Select(membership => membership.ToInviteDto())
            .ToList();
    }
}
