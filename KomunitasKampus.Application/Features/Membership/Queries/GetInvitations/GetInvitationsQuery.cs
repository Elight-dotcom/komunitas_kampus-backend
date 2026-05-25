using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetInvitations;

public sealed record GetInvitationsQuery(
    Guid AccountId
) : IRequest<IReadOnlyList<InviteDto>>;
