using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetPostById;

public sealed record GetPostByIdQuery(
    Guid PostId,
    Guid? ViewerAccountId,
    string? ViewerRole,
    Guid? ViewerOrganizationId
) : IRequest<PostDto>;
