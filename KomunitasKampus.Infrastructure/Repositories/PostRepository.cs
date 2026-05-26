using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace KomunitasKampus.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;
    private readonly IMinioService _minioService;

    public PostRepository(
        AppDbContext context,
        IMinioService minioService
    )
    {
        _context = context;
        _minioService = minioService;
    }

    public async Task<IReadOnlyList<Post>> GetFeedAsync(
        Guid organizationId,
        Guid? viewerAccountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var isOrganizationOwner = false;
        var isAcceptedMember = false;

        if (viewerAccountId.HasValue)
        {
            isOrganizationOwner = await _context.Organizations
                .AnyAsync(
                    organization =>
                        organization.Id == organizationId &&
                        organization.AccountId == viewerAccountId.Value,
                    cancellationToken
                );

            if (!isOrganizationOwner)
            {
                isAcceptedMember = await IsAcceptedMemberAsync(
                    organizationId,
                    viewerAccountId.Value,
                    cancellationToken
                );
            }
        }

        var query = _context.Posts
            .AsNoTracking()
            .Include(post => post.Organization)
            .Include(post => post.Media.OrderBy(media => media.OrderIndex))
            .Where(post => post.OrganizationId == organizationId);

        if (!isOrganizationOwner)
        {
            query = isAcceptedMember
                ? query.Where(post =>
                    post.Visibility == PostVisibility.Public ||
                    post.Visibility == PostVisibility.Internal)
                : query.Where(post => post.Visibility == PostVisibility.Public);
        }

        return await query
            .OrderByDescending(post => post.IsPinned)
            .ThenBy(post => post.IsPinned ? post.PinOrder ?? int.MaxValue : int.MaxValue)
            .ThenByDescending(post => post.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Posts
            .Include(post => post.Organization)
            .Include(post => post.Media.OrderBy(media => media.OrderIndex))
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken);
    }

    public async Task CreateAsync(
        Post post,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Posts.AddAsync(post, cancellationToken);
    }

    public Task UpdateAsync(
        Post post,
        CancellationToken cancellationToken = default
    )
    {
        _context.Posts.Update(post);

        return Task.CompletedTask;
    }

    public async Task DeleteAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        var post = await _context.Posts
            .Include(entity => entity.Media)
            .FirstOrDefaultAsync(entity => entity.Id == postId, cancellationToken);

        if (post is null)
        {
            return;
        }

        foreach (var media in post.Media)
        {
            if (!string.IsNullOrWhiteSpace(media.FileUrl))
            {
                await _minioService.DeleteFileAsync(media.FileUrl, cancellationToken);
            }

            media.DeletedAt = DateTime.UtcNow;
            media.UpdatedAt = DateTime.UtcNow;
        }

        post.DeletedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
    }

    public Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        PostMediaType mediaType,
        CancellationToken cancellationToken = default
    )
    {
        var contentType = GetContentType(fileName, mediaType);

        return _minioService.GeneratePresignedPutUrlAsync(
            fileName,
            contentType,
            cancellationToken
        );
    }

    public async Task<int> CountPinnedPostsAsync(
        Guid organizationId,
        Guid? excludedPostId = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Posts
            .Where(post =>
                post.OrganizationId == organizationId &&
                post.IsPinned);

        if (excludedPostId.HasValue)
        {
            query = query.Where(post => post.Id != excludedPostId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> IsAcceptedMemberAsync(
        Guid organizationId,
        Guid viewerAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Memberships
            .AnyAsync(
                membership =>
                    membership.OrganizationId == organizationId &&
                    membership.AccountId == viewerAccountId &&
                    membership.Status == MembershipStatus.Accepted,
                cancellationToken
            );
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string GetContentType(
        string fileName,
        PostMediaType mediaType
    )
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return mediaType switch
        {
            PostMediaType.Image when extension is ".jpg" or ".jpeg" => "image/jpeg",
            PostMediaType.Image when extension == ".png" => "image/png",
            PostMediaType.Video => "video/mp4",
            PostMediaType.Document => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
