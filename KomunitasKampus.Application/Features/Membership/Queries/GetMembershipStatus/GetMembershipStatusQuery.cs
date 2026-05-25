using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMembershipStatus;

public sealed record GetMembershipStatusQuery(
    Guid AccountId,
    Guid OrganizationId
) : IRequest<MembershipStatusDto>;
