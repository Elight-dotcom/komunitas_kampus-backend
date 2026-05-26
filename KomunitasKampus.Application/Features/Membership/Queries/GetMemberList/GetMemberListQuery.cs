using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetMemberList;

public sealed record GetMemberListQuery(
    Guid OrganizationId
) : IRequest<IReadOnlyList<MembershipDto>>;
