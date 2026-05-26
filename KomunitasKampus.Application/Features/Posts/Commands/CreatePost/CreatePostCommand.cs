using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.CreatePost;

public sealed record CreatePostCommand(
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId,
    CreatePostDto Payload
) : IRequest<CreatePostResponse>;
