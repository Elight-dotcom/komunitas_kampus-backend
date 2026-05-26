using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Queries.GetSentInvitations;

public record GetSentInvitationsQuery(Guid OrganizationId)
    : IRequest<IReadOnlyList<SentInvitationDto>>;