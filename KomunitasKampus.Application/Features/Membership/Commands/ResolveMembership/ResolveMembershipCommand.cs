using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.ResolveMembership;

public sealed record ResolveMembershipCommand(
    Guid MembershipId,
    string Action,
    Guid ResolvedBy
) : IRequest<MembershipDto>;
