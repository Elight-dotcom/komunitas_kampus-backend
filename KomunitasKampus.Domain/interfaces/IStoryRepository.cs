using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IStoryRepository
{
    Task<IReadOnlyList<Story>> GetActiveStoriesAsync(
        Guid? viewerAccountId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Story>> GetActiveStoriesByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<Story?> GetByIdAsync(
        Guid storyId,
        CancellationToken cancellationToken = default
    );

    Task CreateAsync(
        Story story,
        CancellationToken cancellationToken = default
    );

    Task MarkAsViewedAsync(
        Guid storyId,
        Guid accountId,
        CancellationToken cancellationToken = default
    );

    Task<int> GetActiveCountByOrgAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    );

    Task<int> ExpireStoriesAsync(
        CancellationToken cancellationToken = default
    );
}
