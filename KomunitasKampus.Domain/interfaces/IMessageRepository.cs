using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IMessageRepository
{
    Task<List<Message>> GetMessagesByRoomIdAsync(Guid roomId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);

    Task<Message?> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
}
