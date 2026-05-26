using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly AppDbContext _context;

    public LikeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ToggleLikeAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(
                like =>
                    like.UserId == userId &&
                    like.PostId == postId,
                cancellationToken
            );

        if (existingLike is not null)
        {
            await _context.Likes
                .Where(like => like.Id == existingLike.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await _context.Posts
                .Where(post => post.Id == postId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        post => post.LikeCount,
                        post => post.LikeCount > 0 ? post.LikeCount - 1 : 0
                    ),
                    cancellationToken
                );

            await transaction.CommitAsync(cancellationToken);

            return false;
        }

        // Bersihkan kemungkinan row lama yang pernah soft-deleted oleh kode sebelumnya,
        // supaya unique index user_id + post_id tetap aman saat re-like.
        await _context.Likes
            .IgnoreQueryFilters()
            .Where(like =>
                like.UserId == userId &&
                like.PostId == postId
            )
            .ExecuteDeleteAsync(cancellationToken);

        var like = new Like
        {
            UserId = userId,
            PostId = postId
        };

        await _context.Likes.AddAsync(like, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Posts
            .Where(post => post.Id == postId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    post => post.LikeCount,
                    post => post.LikeCount + 1
                ),
                cancellationToken
            );

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<int> GetLikeCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Likes
            .CountAsync(
                like => like.PostId == postId,
                cancellationToken
            );
    }

    public async Task<bool> IsLikedByUserAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Likes
            .AnyAsync(
                like =>
                    like.UserId == userId &&
                    like.PostId == postId,
                cancellationToken
            );
    }
}
