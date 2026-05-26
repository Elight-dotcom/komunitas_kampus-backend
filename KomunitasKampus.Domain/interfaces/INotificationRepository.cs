using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default
    );

    Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default
    );
}