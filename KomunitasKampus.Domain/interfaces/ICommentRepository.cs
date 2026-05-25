using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Domain.Interfaces;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetByPostIdAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<Comment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default
    );

    Task CreateAsync(
        Comment comment,
        CancellationToken cancellationToken = default
    );

    Task SoftDeleteAsync(
        Guid commentId,
        Guid deletedById,
        string? deletedReason,
        CancellationToken cancellationToken = default
    );

    Task<int> GetCommentCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task<int> CountRecentByUserForPostAsync(
        Guid userId,
        Guid postId,
        DateTime fromUtc,
        CancellationToken cancellationToken = default
    );
}
