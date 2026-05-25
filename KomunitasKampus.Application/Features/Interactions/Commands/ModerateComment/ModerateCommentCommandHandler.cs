using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Interactions.Commands.ModerateComment;

public class ModerateCommentCommandHandler
    : IRequestHandler<ModerateCommentCommand, ModerateCommentResultDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotificationService _notificationService;
    private readonly IInteractionRealtimeNotifier _realtimeNotifier;

    public ModerateCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        INotificationService notificationService,
        IInteractionRealtimeNotifier realtimeNotifier
    )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _notificationService = notificationService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<ModerateCommentResultDto> Handle(
        ModerateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var post = await _postRepository.GetByIdAsync(
            request.PostId,
            cancellationToken
        );

        InteractionAccessRules.EnsurePostExists(post);

        if (request.RequesterOrganizationId != post!.OrganizationId)
        {
            throw new ForbiddenAccessAppException(
                "Admin organisasi hanya boleh memoderasi komentar pada postingan organisasinya sendiri."
            );
        }

        var comment = await _commentRepository.GetByIdAsync(
            request.CommentId,
            cancellationToken
        );

        if (comment is null)
        {
            throw new NotFoundAppException("Komentar tidak ditemukan.");
        }

        if (comment.PostId != request.PostId)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["comment"] = new[]
                    {
                        "Komentar tidak sesuai dengan postingan yang diminta."
                    }
                }
            );
        }

        if (comment.DeletedAt is not null)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["comment"] = new[]
                    {
                        "Komentar sudah dihapus."
                    }
                }
            );
        }

        var reason = request.DeletedReason.Trim();

        await _commentRepository.SoftDeleteAsync(
            request.CommentId,
            request.RequesterAccountId,
            reason,
            cancellationToken
        );

        post.CommentCount = Math.Max(0, post.CommentCount - 1);
        await _postRepository.UpdateAsync(post, cancellationToken);

        var commentCount = await _commentRepository.GetCommentCountAsync(
            request.PostId,
            cancellationToken
        );

        post.CommentCount = commentCount;
        await _postRepository.UpdateAsync(post, cancellationToken);

        await _notificationService.SendNotificationAsync(
            recipientId: comment.UserId,
            actorId: request.RequesterAccountId,
            type: "comment_moderated",
            referenceId: comment.Id,
            cancellationToken: cancellationToken
        );

        await _realtimeNotifier.BroadcastCommentDeletedAsync(
            postId: request.PostId,
            commentId: request.CommentId,
            isModerated: true,
            deletedReason: reason,
            commentCount: commentCount,
            cancellationToken: cancellationToken
        );

        return new ModerateCommentResultDto(
            PostId: request.PostId,
            CommentId: request.CommentId,
            DeletedReason: reason,
            CommentCount: commentCount
        );
    }
}
