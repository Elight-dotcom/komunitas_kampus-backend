using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ToggleLike;

public sealed record ToggleLikeCommand(
    Guid UserId,
    Guid PostId
) : IRequest<LikeStatusDto>;
