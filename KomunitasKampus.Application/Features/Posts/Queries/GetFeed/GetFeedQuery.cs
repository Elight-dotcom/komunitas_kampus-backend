using KomunitasKampus.Application.Features.Posts.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetFeed;

public sealed record GetFeedQuery(
    Guid OrganizationId,
    Guid? ViewerAccountId,
    string? ViewerRole,
    Guid? ViewerOrganizationId,
    int Page = 1,
    int PageSize = 10
) : IRequest<IReadOnlyList<PostDto>>;
