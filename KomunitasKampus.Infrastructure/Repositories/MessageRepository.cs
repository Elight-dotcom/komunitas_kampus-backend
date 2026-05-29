using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetMessagesByRoomIdAsync(
        Guid roomId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (safePage - 1) * safePageSize;

        var messages = await _context.Messages
            .Include(message => message.Sender)
            .Where(message => message.RoomId == roomId)
            .OrderByDescending(message => message.SentAt)
            .Skip(skip)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        messages.Reverse();

        return messages;
    }

    public async Task<Message> CreateAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Messages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Messages
            .Include(createdMessage => createdMessage.Sender)
            .FirstAsync(
                createdMessage => createdMessage.Id == message.Id,
                cancellationToken
            );
    }

    public async Task<Message?> GetByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Messages
            .Include(message => message.Sender)
            .Include(message => message.DeletedBy)
            .Include(message => message.Room)
                .ThenInclude(room => room!.Organization)
                    .ThenInclude(organization => organization!.Account)
            .FirstOrDefaultAsync(
                message => message.Id == messageId,
                cancellationToken
            );
    }

    public async Task UpdateAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
