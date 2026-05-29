namespace KomunitasKampus.Domain.Interfaces;

public interface IChatReadStatusRepository
{
    Task UpsertReadStatusAsync(Guid roomId, Guid accountId, DateTime readAt, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid accountId, List<Guid> roomIds, CancellationToken cancellationToken = default);
}
