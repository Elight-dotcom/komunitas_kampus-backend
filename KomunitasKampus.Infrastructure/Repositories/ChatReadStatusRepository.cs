using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class ChatReadStatusRepository : IChatReadStatusRepository
{
    private readonly AppDbContext _context;

    public ChatReadStatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpsertReadStatusAsync(
        Guid roomId,
        Guid accountId,
        DateTime readAt,
        CancellationToken cancellationToken = default
    )
    {
        var existingReadStatus = await _context.ChatReadStatuses
            .FirstOrDefaultAsync(
                status =>
                    status.RoomId == roomId &&
                    status.AccountId == accountId,
                cancellationToken
            );

        if (existingReadStatus is null)
        {
            await _context.ChatReadStatuses.AddAsync(
                new ChatReadStatus
                {
                    RoomId = roomId,
                    AccountId = accountId,
                    LastReadAt = readAt
                },
                cancellationToken
            );
        }
        else
        {
            existingReadStatus.LastReadAt = readAt;
            existingReadStatus.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetUnreadCountsAsync(
        Guid accountId,
        List<Guid> roomIds,
        CancellationToken cancellationToken = default
    )
    {
        if (roomIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var readStatuses = await _context.ChatReadStatuses
            .Where(status =>
                status.AccountId == accountId &&
                roomIds.Contains(status.RoomId)
            )
            .ToDictionaryAsync(
                status => status.RoomId,
                status => status.LastReadAt,
                cancellationToken
            );

        var unreadCounts = new Dictionary<Guid, int>();

        foreach (var roomId in roomIds.Distinct())
        {
            var hasReadStatus = readStatuses.TryGetValue(
                roomId,
                out var lastReadAt
            );

            var query = _context.Messages
                .Where(message =>
                    message.RoomId == roomId &&
                    message.SenderId != accountId &&
                    message.DeletedAt == null
                );

            if (hasReadStatus)
            {
                query = query.Where(message => message.SentAt > lastReadAt);
            }

            unreadCounts[roomId] = await query.CountAsync(cancellationToken);
        }

        return unreadCounts;
    }
}
