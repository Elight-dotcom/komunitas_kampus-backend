using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.SharePost;

public class SharePostCommandHandler
    : IRequestHandler<SharePostCommand, ShareResultDto>
{
    private readonly IShareRepository _shareRepository;
    private readonly IPostRepository _postRepository;

    public SharePostCommandHandler(
        IShareRepository shareRepository,
        IPostRepository postRepository
    )
    {
        _shareRepository = shareRepository;
        _postRepository = postRepository;
    }

    public async Task<ShareResultDto> Handle(
        SharePostCommand request,
        CancellationToken cancellationToken
    )
    {
        var post = await _postRepository.GetByIdAsync(
            request.PostId,
            cancellationToken
        );

        InteractionAccessRules.EnsurePostExists(post);

        var share = new Share
        {
            UserId = request.UserId,
            PostId = request.PostId,
            Platform = request.Platform,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _shareRepository.CreateAsync(share, cancellationToken);

        post!.ShareCount = Math.Max(0, post.ShareCount + 1);
        await _postRepository.UpdateAsync(post, cancellationToken);

        var shareCount = await _shareRepository.GetShareCountAsync(
            request.PostId,
            cancellationToken
        );

        post.ShareCount = shareCount;
        await _postRepository.UpdateAsync(post, cancellationToken);

        var requiresAuth =
            request.Platform == SharePlatform.External &&
            InteractionAccessRules.IsInternalPost(post);

        return new ShareResultDto(
            ShareCount: shareCount,
            RequiresAuth: requiresAuth
        );
    }
}
