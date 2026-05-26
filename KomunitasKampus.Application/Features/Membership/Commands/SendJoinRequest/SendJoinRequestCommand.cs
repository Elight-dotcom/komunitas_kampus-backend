using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendJoinRequest;

public sealed record SendJoinRequestCommand(
    Guid AccountId,
    Guid OrganizationId
) : IRequest<MembershipDto>;
