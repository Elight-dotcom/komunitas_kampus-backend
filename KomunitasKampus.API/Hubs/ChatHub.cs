using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KomunitasKampus.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var accountId = GetAccountId();

        if (accountId.HasValue)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"account_{accountId.Value}"
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
                $"account_{accountId.Value}"
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(Guid roomId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"room_{roomId}"
        );
    }

    public async Task LeaveRoom(Guid roomId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"room_{roomId}"
        );
    }

    public async Task SendMessageToRoom(Guid roomId, object message)
    {
        await Clients.Group($"room_{roomId}").SendAsync("ReceiveMessage", message);
    }

    public async Task NotifyTyping(Guid roomId, Guid accountId, string username)
    {
        await Clients.Group($"room_{roomId}").SendAsync("UserTyping", new
        {
            roomId,
            accountId,
            username
        });
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