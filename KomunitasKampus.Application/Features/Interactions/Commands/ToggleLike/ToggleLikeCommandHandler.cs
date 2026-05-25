using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, LikeStatusDto>
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IInteractionRealtimeNotifier _realtimeNotifier;

    public ToggleLikeCommandHandler(
        ILikeRepository likeRepository,
        IPostRepository postRepository,
        IAccountRepository accountRepository,
        IInteractionRealtimeNotifier realtimeNotifier
    )
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _accountRepository = accountRepository;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<LikeStatusDto> Handle(
        ToggleLikeCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await _accountRepository.GetByIdAsync(
            request.UserId,
            cancellationToken
        );

        InteractionAccessRules.EnsureAccountExists(account);

        var post = await _postRepository.GetByIdAsync(
            request.PostId,
            cancellationToken
        );

        InteractionAccessRules.EnsurePostExists(post);
        InteractionAccessRules.EnsureNotOwnOrganizationPost(
            account!,
            post!,
            "like"
        );

        var isLiked = await _likeRepository.ToggleLikeAsync(
            request.UserId,
            request.PostId,
            cancellationToken
        );

        post!.LikeCount = Math.Max(
            0,
            post.LikeCount + (isLiked ? 1 : -1)
        );

        await _postRepository.UpdateAsync(post, cancellationToken);

        var likeCount = await _likeRepository.GetLikeCountAsync(
            request.PostId,
            cancellationToken
        );

        post.LikeCount = likeCount;
        await _postRepository.UpdateAsync(post, cancellationToken);

        await _realtimeNotifier.BroadcastLikeUpdatedAsync(
            request.PostId,
            request.UserId,
            isLiked,
            likeCount,
            cancellationToken
        );

        return new LikeStatusDto(
            IsLiked: isLiked,
            LikeCount: likeCount
        );
    }
}
