using KomunitasKampus.Application.Features.Posts;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetGlobalFeed;

public class GetGlobalFeedQueryHandler : IRequestHandler<GetGlobalFeedQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostRepository _postRepository;

    public GetGlobalFeedQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IReadOnlyList<PostDto>> Handle(
        GetGlobalFeedQuery request,
        CancellationToken cancellationToken
    )
    {
        var posts = await _postRepository.GetGlobalFeedAsync(
            request.ViewerAccountId,
            request.ViewerRole,
            request.ViewerOrganizationId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return posts
            .OrderByDescending(post => post.IsPinned)
            .ThenBy(post => post.IsPinned ? post.PinOrder ?? int.MaxValue : int.MaxValue)
            .ThenByDescending(post => post.CreatedAt)
            .Select(post => post.ToDto())
            .ToList();
    }
}