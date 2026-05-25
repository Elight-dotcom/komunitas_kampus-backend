using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.CreateComment;

public class CreateCommentCommandHandler
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private const int MaxCommentPerMinute = 5;

    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IInteractionRealtimeNotifier _realtimeNotifier;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IAccountRepository accountRepository,
        IInteractionRealtimeNotifier realtimeNotifier
    )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _accountRepository = accountRepository;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
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
            "comment"
        );

        var recentCommentCount = await _commentRepository.CountRecentByUserForPostAsync(
            request.UserId,
            request.PostId,
            DateTime.UtcNow.AddMinutes(-1),
            cancellationToken
        );

        if (recentCommentCount >= MaxCommentPerMinute)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["comment"] = new[]
                    {
                        "Kamu terlalu sering mengirim komentar. Maksimal 5 komentar dalam 1 menit untuk post yang sama."
                    }
                }
            );
        }

        var comment = new Comment
        {
            UserId = request.UserId,
            PostId = request.PostId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = account
        };

        await _commentRepository.CreateAsync(comment, cancellationToken);

        post!.CommentCount = Math.Max(0, post.CommentCount + 1);
        await _postRepository.UpdateAsync(post, cancellationToken);

        var commentCount = await _commentRepository.GetCommentCountAsync(
            request.PostId,
            cancellationToken
        );

        post.CommentCount = commentCount;
        await _postRepository.UpdateAsync(post, cancellationToken);

        var dto = comment.ToCommentDto();

        await _realtimeNotifier.BroadcastCommentAddedAsync(
            request.PostId,
            dto,
            commentCount,
            cancellationToken
        );

        return dto;
    }
}
