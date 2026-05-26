using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IOrganizationRepository
{
    Task<IReadOnlyList<Organization>> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Organization>> GetRecommendedAsync(
        int limit,
        CancellationToken cancellationToken = default
    );

    Task<Organization?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}
