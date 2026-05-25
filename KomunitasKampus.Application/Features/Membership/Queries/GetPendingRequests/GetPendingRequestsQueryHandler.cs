using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetPendingRequests;

public class GetPendingRequestsQueryHandler
    : IRequestHandler<GetPendingRequestsQuery, IReadOnlyList<MemberRequestDto>>
{
    private readonly IMembershipRepository _membershipRepository;

    public GetPendingRequestsQueryHandler(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<IReadOnlyList<MemberRequestDto>> Handle(
        GetPendingRequestsQuery request,
        CancellationToken cancellationToken
    )
    {
        var pendingRequests = await _membershipRepository.GetPendingRequestsAsync(
            request.OrganizationId,
            cancellationToken
        );

        return pendingRequests
            .Select(membership => membership.ToMemberRequestDto())
            .ToList();
    }
}
