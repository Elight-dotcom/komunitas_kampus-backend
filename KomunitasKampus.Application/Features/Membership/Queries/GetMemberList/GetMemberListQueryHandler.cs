using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMemberList;

public class GetMemberListQueryHandler
    : IRequestHandler<GetMemberListQuery, IReadOnlyList<MembershipDto>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetMemberListQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<IReadOnlyList<MembershipDto>> Handle(
        GetMemberListQuery request,
        CancellationToken cancellationToken
    )
    {
        var members = await _membershipRepository.GetMemberListAsync(
            request.OrganizationId,
            cancellationToken
        );

        return members
            .Select(membership => membership.ToMembershipDto())
            .ToList();
    }
}
