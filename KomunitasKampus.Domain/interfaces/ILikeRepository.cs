namespace KomunitasKampus.Domain.Interfaces;

public interface ILikeRepository
{
    Task<bool> ToggleLikeAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task<int> GetLikeCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsLikedByUserAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    );
}
