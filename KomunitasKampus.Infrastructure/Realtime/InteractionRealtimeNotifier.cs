using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Interactions.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace KomunitasKampus.Infrastructure.Realtime;

public class InteractionRealtimeNotifier : IInteractionRealtimeNotifier
{
    private readonly IHubContext<AppHub> _hubContext;

    public InteractionRealtimeNotifier(IHubContext<AppHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastLikeUpdatedAsync(
        Guid postId,
        Guid actorId,
        bool isLiked,
        int likeCount,
        CancellationToken cancellationToken = default
    )
    {
        await _hubContext.Clients
            .Group(AppHub.GetPostGroupName(postId))
            .SendAsync(
                "LikeUpdated",
                new
                {
                    postId,
                    actorId,
                    isLiked,
                    likeCount
                },
                cancellationToken
            );
    }

    public async Task BroadcastCommentAddedAsync(
        Guid postId,
        CommentDto comment,
        int commentCount,
        CancellationToken cancellationToken = default
    )
    {
        await _hubContext.Clients
            .Group(AppHub.GetPostGroupName(postId))
            .SendAsync(
                "CommentAdded",
                new
                {
                    postId,
                    comment,
                    commentCount
                },
                cancellationToken
            );
    }

    public async Task BroadcastCommentDeletedAsync(
        Guid postId,
        Guid commentId,
        bool isModerated,
        string? deletedReason,
        int commentCount,
        CancellationToken cancellationToken = default
    )
    {
        await _hubContext.Clients
            .Group(AppHub.GetPostGroupName(postId))
            .SendAsync(
                isModerated ? "CommentModerated" : "CommentDeleted",
                new
                {
                    postId,
                    commentId,
                    isModerated,
                    deletedReason,
                    commentCount
                },
                cancellationToken
            );
    }
}
