using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<AppHub> _hubContext;

    public ChatService(
        AppDbContext context,
        IHubContext<AppHub> hubContext
    )
    {
        _context = context;
        _hubContext = hubContext;
    }

    public Task JoinMainGroupAsync(
        Guid organizationId,
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return AddMemberToMainGroupAsync(
            accountId,
            organizationId,
            cancellationToken
        );
    }

    public async Task AddMemberToMainGroupAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        var mainRoom = await _context.ChatRooms
            .FirstOrDefaultAsync(
                room =>
                    room.OrganizationId == organizationId &&
                    room.IsMainGroup,
                cancellationToken
            );

        if (mainRoom is null)
        {
            return;
        }

        var alreadyParticipant = await _context.ChatParticipants
            .AnyAsync(
                participant =>
                    participant.RoomId == mainRoom.Id &&
                    participant.AccountId == accountId,
                cancellationToken
            );

        if (!alreadyParticipant)
        {
            var participant = new ChatParticipant
            {
                RoomId = mainRoom.Id,
                AccountId = accountId,
                JoinedAt = DateTime.UtcNow
            };

            await _context.ChatParticipants.AddAsync(participant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _hubContext.Clients
            .Group(RealtimeGroups.Account(accountId))
            .SendAsync(
                "chat_list_refresh",
                new
                {
                    roomId = mainRoom.Id,
                    organizationId,
                    joinedAt = DateTime.UtcNow
                },
                cancellationToken
            );
    }
}
