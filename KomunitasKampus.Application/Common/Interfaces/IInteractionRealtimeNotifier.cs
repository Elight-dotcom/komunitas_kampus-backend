using KomunitasKampus.Application.Features.Interactions.DTOs;

namespace KomunitasKampus.Application.Common.Interfaces;

public interface IInteractionRealtimeNotifier
{
    Task BroadcastLikeUpdatedAsync(
        Guid postId,
        Guid actorId,
        bool isLiked,
        int likeCount,
        CancellationToken cancellationToken = default
    );

    Task BroadcastCommentAddedAsync(
        Guid postId,
        CommentDto comment,
        int commentCount,
        CancellationToken cancellationToken = default
    );

    Task BroadcastCommentDeletedAsync(
        Guid postId,
        Guid commentId,
        bool isModerated,
        string? deletedReason,
        int commentCount,
        CancellationToken cancellationToken = default
    );

}
