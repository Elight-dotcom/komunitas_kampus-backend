using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KomunitasKampus.Infrastructure.Realtime;

[Authorize]
public class AppHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var accountId = GetAccountId();

        if (accountId.HasValue)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                RealtimeGroups.Account(accountId.Value)
            );
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var accountId = GetAccountId();

        if (accountId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                RealtimeGroups.Account(accountId.Value)
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinPostGroup(Guid postId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetPostGroupName(postId)
        );
    }

    public async Task LeavePostGroup(Guid postId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetPostGroupName(postId)
        );
    }

    public async Task BroadcastLikeUpdate(
        Guid postId,
        int likeCount,
        bool isLiked
    )
    {
        await Clients.Group(GetPostGroupName(postId))
            .SendAsync(
                "LikeUpdated",
                new
                {
                    postId,
                    likeCount,
                    isLiked
                }
            );
    }

    public async Task BroadcastCommentAdded(
        Guid postId,
        object commentDto
    )
    {
        await Clients.Group(GetPostGroupName(postId))
            .SendAsync(
                "CommentAdded",
                new
                {
                    postId,
                    comment = commentDto
                }
            );
    }

    public async Task BroadcastCommentModerated(
        Guid postId,
        Guid commentId,
        string deletedReason
    )
    {
        await Clients.Group(GetPostGroupName(postId))
            .SendAsync(
                "CommentModerated",
                new
                {
                    postId,
                    commentId,
                    deletedReason
                }
            );
    }

    public static string GetPostGroupName(Guid postId)
    {
        return $"post_{postId}";
    }

    private Guid? GetAccountId()
    {
        var value =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            Context.User?.FindFirstValue("sub") ??
            Context.User?.FindFirstValue("account_id");

        return Guid.TryParse(value, out var accountId)
            ? accountId
            : null;
    }
}
