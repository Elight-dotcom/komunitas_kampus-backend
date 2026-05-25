using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface IShareRepository
{
    Task CreateAsync(
        Share share,
        CancellationToken cancellationToken = default
    );

    Task<int> GetShareCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );
}
