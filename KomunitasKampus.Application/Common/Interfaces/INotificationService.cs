namespace KomunitasKampus.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendMembershipNotificationAsync(
        Guid accountId,
        Guid organizationId,
        Guid membershipId,
        string notificationType,
        CancellationToken cancellationToken = default
    );

    Task SendNotificationAsync(
        Guid recipientId,
        Guid? actorId,
        string type,
        Guid? referenceId,
        CancellationToken cancellationToken = default
    );
}
