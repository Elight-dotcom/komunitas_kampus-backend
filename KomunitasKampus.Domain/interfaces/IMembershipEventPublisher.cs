namespace KomunitasKampus.Domain.Interfaces;

public interface IMembershipEventPublisher
{
    Task PublishMemberAcceptedAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );
}
