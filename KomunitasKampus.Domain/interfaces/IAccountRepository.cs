using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    );

    Task<Account?> GetByEmailOrUsernameAsync(
        string emailOrUsername,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsEmailTakenAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsUsernameTakenAsync(
        string username,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Account>> SearchStudentAccountsByUsernameAsync(
        string username,
        int limit,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsOrganizationNameTakenAsync(
        string organizationName,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}