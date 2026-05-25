using KomunitasKampus.Application.Features.Membership.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendInvite;

public sealed record SendInviteCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    Guid OrganizationId,
    string TargetUsername
) : IRequest<InviteDto>;
