namespace KomunitasKampus.Application.Common.Interfaces;

public interface IChatService
{
    Task JoinMainGroupAsync(
        Guid organizationId,
        Guid accountId,
        CancellationToken cancellationToken = default
    );

    Task AddMemberToMainGroupAsync(
        Guid accountId,
        Guid organizationId,
        CancellationToken cancellationToken = default
    );
}
