using KomunitasKampus.Application.Features.Interactions.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Queries.GetLikeStatus;

public sealed record GetLikeStatusQuery(
    Guid UserId,
    Guid PostId
) : IRequest<LikeStatusDto>;
