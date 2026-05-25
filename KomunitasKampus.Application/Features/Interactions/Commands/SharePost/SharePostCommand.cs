using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Enums;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.SharePost;

public sealed record SharePostCommand(
    Guid UserId,
    Guid PostId,
    SharePlatform Platform
) : IRequest<ShareResultDto>;
