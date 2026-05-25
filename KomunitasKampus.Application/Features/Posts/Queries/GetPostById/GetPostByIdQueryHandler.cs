using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Posts.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            throw new NotFoundAppException("Postingan tidak ditemukan.");
        }

        var isAcceptedMember = request.ViewerAccountId.HasValue
            && await _postRepository.IsAcceptedMemberAsync(
                post.OrganizationId,
                request.ViewerAccountId.Value,
                cancellationToken
            );

        var canView = PostAccessRules.CanViewPost(
            post,
            request.ViewerAccountId,
            request.ViewerOrganizationId,
            request.ViewerRole,
            isAcceptedMember
        );

        if (!canView)
        {
            throw new ForbiddenAccessAppException("Kamu tidak punya akses untuk melihat postingan ini.");
        }

        return post.ToDto();
    }
}
