using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Interfaces;

public interface IPostRepository
{
    Task<IReadOnlyList<Post>> GetFeedAsync(
        Guid organizationId,
        Guid? viewerAccountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<Post?> GetByIdAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task CreateAsync(
        Post post,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Post post,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        PostMediaType mediaType,
        CancellationToken cancellationToken = default
    );

    Task<int> CountPinnedPostsAsync(
        Guid organizationId,
        Guid? excludedPostId = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsAcceptedMemberAsync(
        Guid organizationId,
        Guid viewerAccountId,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}
