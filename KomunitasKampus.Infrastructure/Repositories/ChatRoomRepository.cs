using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly AppDbContext _context;

    public ChatRoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetByIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChatRooms
            .Include(room => room.Organization)
                .ThenInclude(organization => organization!.Account)
            .Include(room => room.Participants)
                .ThenInclude(participant => participant.Account)
            .Include(room => room.Messages
                .OrderByDescending(message => message.SentAt)
                .Take(1))
                .ThenInclude(message => message.Sender)
            .FirstOrDefaultAsync(
                room => room.Id == roomId,
                cancellationToken
            );
    }

    public async Task<List<ChatRoom>> GetRoomsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChatParticipants
            .Where(participant => participant.AccountId == accountId)
            .Select(participant => participant.ChatRoom!)
            .Include(room => room.Organization)
                .ThenInclude(organization => organization!.Account)
            .Include(room => room.Participants)
                .ThenInclude(participant => participant.Account)
            .Include(room => room.Messages
                .OrderByDescending(message => message.SentAt)
                .Take(1))
                .ThenInclude(message => message.Sender)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatRoom?> GetMainGroupByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChatRooms
            .Include(room => room.Participants)
                .ThenInclude(participant => participant.Account)
            .FirstOrDefaultAsync(
                room =>
                    room.OrganizationId == organizationId &&
                    room.IsMainGroup,
                cancellationToken
            );
    }

    public async Task<ChatRoom?> GetExistingDirectRoomAsync(
        Guid accountId1,
        Guid accountId2,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChatRooms
            .Include(room => room.Participants)
                .ThenInclude(participant => participant.Account)
            .Where(room =>
                room.RoomType == ChatRoomType.Direct &&
                room.Participants.Any(participant =>
                    participant.AccountId == accountId1
                ) &&
                room.Participants.Any(participant =>
                    participant.AccountId == accountId2
                )
            )
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChatRoom> CreateAsync(
        ChatRoom room,
        CancellationToken cancellationToken = default
    )
    {
        await _context.ChatRooms.AddAsync(room, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return room;
    }

    public async Task AddParticipantAsync(
        ChatParticipant participant,
        CancellationToken cancellationToken = default
    )
    {
        var alreadyExists = await _context.ChatParticipants
            .AnyAsync(
                existing =>
                    existing.RoomId == participant.RoomId &&
                    existing.AccountId == participant.AccountId,
                cancellationToken
            );

        if (alreadyExists)
        {
            return;
        }

        await _context.ChatParticipants.AddAsync(participant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveParticipantAsync(
        Guid roomId,
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        var participant = await _context.ChatParticipants
            .FirstOrDefaultAsync(
                item =>
                    item.RoomId == roomId &&
                    item.AccountId == accountId,
                cancellationToken
            );

        if (participant is null)
        {
            return;
        }

        _context.ChatParticipants.Remove(participant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsParticipantAsync(
        Guid roomId,
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChatParticipants
            .AnyAsync(
                participant =>
                    participant.RoomId == roomId &&
                    participant.AccountId == accountId,
                cancellationToken
            );
    }
}
