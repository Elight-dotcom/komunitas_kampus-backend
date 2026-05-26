using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetPendingRequests;

public sealed record GetPendingRequestsQuery(
    Guid OrganizationId
) : IRequest<IReadOnlyList<MemberRequestDto>>;
