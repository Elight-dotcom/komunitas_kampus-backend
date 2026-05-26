using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetSentInvitations;

public class GetSentInvitationsQueryHandler
    : IRequestHandler<GetSentInvitationsQuery, IReadOnlyList<SentInvitationDto>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetSentInvitationsQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<IReadOnlyList<SentInvitationDto>> Handle(
        GetSentInvitationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var invitations = await _membershipRepository.GetSentInvitationsAsync(
            request.OrganizationId,
            cancellationToken
        );

        return invitations
            .Select(m => new SentInvitationDto(
                m.Id,
                m.AccountId,
                m.OrganizationId,
                m.Account?.Username,
                m.Account?.Email,
                m.Account?.User?.FullName,
                m.Account?.User?.University,
                m.Status.ToString(),
                m.RequestedAt,
                m.ResolvedAt
            ))
            .ToList();
    }
}
