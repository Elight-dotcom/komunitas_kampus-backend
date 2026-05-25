using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Caching;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;
    private readonly RedisCommentCacheService _commentCache;

    public CommentRepository(
        AppDbContext context,
        RedisCommentCacheService commentCache
    )
    {
        _context = context;
        _commentCache = commentCache;
    }

    public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var cachedComments = await _commentCache.GetCommentsAsync(
            postId,
            page,
            cancellationToken
        );

        if (cachedComments is not null)
        {
            return cachedComments;
        }

        var comments = await _context.Comments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(comment => comment.User)
            .Include(comment => comment.DeletedBy)
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await _commentCache.SetCommentsAsync(
            postId,
            page,
            comments,
            cancellationToken
        );

        return comments;
    }

    public async Task<Comment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Comments
            .IgnoreQueryFilters()
            .Include(comment => comment.User)
            .Include(comment => comment.DeletedBy)
            .FirstOrDefaultAsync(
                comment => comment.Id == commentId,
                cancellationToken
            );
    }

    public async Task CreateAsync(
        Comment comment,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _commentCache.EvictPostCommentsAsync(
            comment.PostId,
            cancellationToken
        );
    }

    public async Task SoftDeleteAsync(
        Guid commentId,
        Guid deletedById,
        string? deletedReason,
        CancellationToken cancellationToken = default
    )
    {
        var comment = await _context.Comments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                item => item.Id == commentId,
                cancellationToken
            );

        if (comment is null || comment.DeletedAt is not null)
        {
            return;
        }

        var utcNow = DateTime.UtcNow;

        comment.DeletedAt = utcNow;
        comment.UpdatedAt = utcNow;
        comment.DeletedById = deletedById;
        comment.DeletedReason = deletedReason;

        await _context.SaveChangesAsync(cancellationToken);

        await _context.Posts
            .Where(post => post.Id == comment.PostId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    post => post.CommentCount,
                    post => post.CommentCount > 0 ? post.CommentCount - 1 : 0
                ),
                cancellationToken
            );

        await _commentCache.EvictPostCommentsAsync(
            comment.PostId,
            cancellationToken
        );
    }

    public async Task<int> GetCommentCountAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Comments
            .CountAsync(
                comment => comment.PostId == postId,
                cancellationToken
            );
    }

    public async Task<int> CountRecentByUserForPostAsync(
        Guid userId,
        Guid postId,
        DateTime fromUtc,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Comments
            .IgnoreQueryFilters()
            .CountAsync(
                comment =>
                    comment.UserId == userId &&
                    comment.PostId == postId &&
                    comment.CreatedAt >= fromUtc,
                cancellationToken
            );
    }
}
