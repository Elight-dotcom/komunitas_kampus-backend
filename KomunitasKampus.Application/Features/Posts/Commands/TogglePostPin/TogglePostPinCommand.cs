using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.TogglePostPin;

public sealed record TogglePostPinCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    Guid OrganizationId,
    Guid PostId,
    bool IsPinned,
    int? PinOrder
) : IRequest<TogglePostPinResponse>;
