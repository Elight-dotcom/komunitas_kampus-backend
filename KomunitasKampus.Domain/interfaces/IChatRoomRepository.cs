using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task<List<ChatRoom>> GetRoomsByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<ChatRoom?> GetMainGroupByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<ChatRoom?> GetExistingDirectRoomAsync(Guid accountId1, Guid accountId2, CancellationToken cancellationToken = default);

    Task<ChatRoom> CreateAsync(ChatRoom room, CancellationToken cancellationToken = default);

    Task AddParticipantAsync(ChatParticipant participant, CancellationToken cancellationToken = default);

    Task RemoveParticipantAsync(Guid roomId, Guid accountId, CancellationToken cancellationToken = default);

    Task<bool> IsParticipantAsync(Guid roomId, Guid accountId, CancellationToken cancellationToken = default);
}
