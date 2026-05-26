using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.RespondToInvite;

public sealed record RespondToInviteCommand(
    Guid MembershipId,
    Guid AccountId,
    string Action
) : IRequest<InviteDto>;
