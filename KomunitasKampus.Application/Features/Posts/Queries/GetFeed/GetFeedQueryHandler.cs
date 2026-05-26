using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetFeed;

public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostRepository _postRepository;

    public GetFeedQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IReadOnlyList<PostDto>> Handle(
        GetFeedQuery request,
        CancellationToken cancellationToken
    )
    {
        var posts = await _postRepository.GetFeedAsync(
            request.OrganizationId,
            request.ViewerAccountId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        var isAcceptedMember = request.ViewerAccountId.HasValue
            && await _postRepository.IsAcceptedMemberAsync(
                request.OrganizationId,
                request.ViewerAccountId.Value,
                cancellationToken
            );

        return posts
            .Where(post => PostAccessRules.CanViewPost(
                post,
                request.ViewerAccountId,
                request.ViewerOrganizationId,
                request.ViewerRole,
                isAcceptedMember
            ))
            .OrderByDescending(post => post.IsPinned)
            .ThenBy(post => post.PinOrder ?? int.MaxValue)
            .ThenByDescending(post => post.CreatedAt)
            .Select(post => post.ToDto())
            .ToList();
    }
}
