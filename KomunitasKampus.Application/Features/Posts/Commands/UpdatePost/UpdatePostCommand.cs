using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.UpdatePost;

public sealed record UpdatePostCommand(
    Guid PostId,
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    UpdatePostDto Payload
) : IRequest<PostDto>;
