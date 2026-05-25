using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Commands.DeletePost;

public sealed record DeletePostCommand(
    Guid PostId,
    Guid RequesterAccountId,
    string RequesterRole,
    Guid? RequesterOrganizationId
) : IRequest<DeletePostResponse>;
