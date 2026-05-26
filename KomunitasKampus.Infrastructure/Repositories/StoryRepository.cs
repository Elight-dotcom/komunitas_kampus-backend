using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Caching;
using KomunitasKampus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KomunitasKampus.Infrastructure.Repositories;

public class StoryRepository : IStoryRepository
{
    private readonly AppDbContext _context;
    private readonly RedisStoryCacheService _storyCache;
    private readonly ILogger<StoryRepository> _logger;

    public StoryRepository(
        AppDbContext context,
        RedisStoryCacheService storyCache,
        ILogger<StoryRepository> logger
    )
    {
        _context = context;
        _storyCache = storyCache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Story>> GetActiveStoriesAsync(
        Guid? viewerAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var cachedStories = await _storyCache.GetActiveStoriesAsync(
            viewerAccountId,
            cancellationToken
        );

        if (cachedStories is not null)
        {
            return cachedStories;
        }

        var utcNow = DateTime.UtcNow;

        var query = _context.Stories
            .AsNoTracking()
            .Include(story => story.Organization)
                .ThenInclude(organization => organization!.Account)
            .Include(story => story.Views.Where(view =>
                !viewerAccountId.HasValue ||
                view.AccountId == viewerAccountId.Value
            ))
            .Where(story =>
                story.IsExpired == false &&
                story.ExpiresAt > utcNow
            );

        if (viewerAccountId.HasValue)
        {
            var accountId = viewerAccountId.Value;

            query = query.Where(story =>
                story.Organization!.AccountId == accountId ||
                _context.Memberships.Any(membership =>
                    membership.AccountId == accountId &&
                    membership.OrganizationId == story.OrganizationId &&
                    membership.Status == MembershipStatus.Accepted
                ) ||
                _context.Posts.Any(post =>
                    post.OrganizationId == story.OrganizationId &&
                    post.Visibility == PostVisibility.Public
                )
            );
        }
        else
        {
            query = query.Where(story =>
                _context.Posts.Any(post =>
                    post.OrganizationId == story.OrganizationId &&
                    post.Visibility == PostVisibility.Public
                )
            );
        }

        var stories = await query
            .OrderBy(story => story.Organization!.OrganizationName)
            .ThenBy(story => story.CreatedAt)
            .ToListAsync(cancellationToken);

        var orderedStories = stories
            .GroupBy(story => story.OrganizationId)
            .Select(group => new
            {
                OrganizationId = group.Key,
                HasUnviewed = !viewerAccountId.HasValue ||
                    group.Any(story => story.Views.All(view =>
                        view.AccountId != viewerAccountId.Value
                    )),
                LatestStoryAt = group.Max(story => story.CreatedAt),
                Stories = group
                    .OrderBy(story => story.CreatedAt)
                    .ToList()
            })
            .OrderByDescending(group => group.HasUnviewed)
            .ThenByDescending(group => group.LatestStoryAt)
            .SelectMany(group => group.Stories)
            .ToList();

        await _storyCache.SetActiveStoriesAsync(
            viewerAccountId,
            orderedStories,
            cancellationToken
        );

        return orderedStories;
    }

    public async Task<Story?> GetByIdAsync(
        Guid storyId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Stories
            .AsNoTracking()
            .Include(story => story.Organization)
                .ThenInclude(organization => organization!.Account)
            .Include(story => story.Views)
            .FirstOrDefaultAsync(
                story => story.Id == storyId,
                cancellationToken
            );
    }

    public async Task CreateAsync(
        Story story,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Stories.AddAsync(story, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _storyCache.EvictActiveStoriesAsync(
            cancellationToken: cancellationToken
        );
    }

    public async Task MarkAsViewedAsync(
        Guid storyId,
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        var alreadyViewed = await _context.StoryViews
            .AnyAsync(
                storyView =>
                    storyView.StoryId == storyId &&
                    storyView.AccountId == accountId,
                cancellationToken
            );

        if (alreadyViewed)
        {
            return;
        }

        var storyView = new StoryView
        {
            StoryId = storyId,
            AccountId = accountId,
            ViewedAt = DateTime.UtcNow
        };

        await _context.StoryViews.AddAsync(storyView, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            await _storyCache.EvictActiveStoriesAsync(
                accountId,
                cancellationToken
            );
        }
        catch (DbUpdateException exception)
        {
            // Upsert pattern: kalau tab lain sudah insert lebih dulu,
            // unique constraint story_id + account_id akan kena.
            // Untuk kasus itu cukup abaikan supaya endpoint idempotent.
            _logger.LogDebug(
                exception,
                "StoryView sudah ada untuk Story {StoryId} dan Account {AccountId}.",
                storyId,
                accountId
            );

            _context.ChangeTracker.Clear();
        }
    }

    public async Task<int> GetActiveCountByOrgAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default
    )
    {
        var utcNow = DateTime.UtcNow;

        return await _context.Stories
            .CountAsync(
                story =>
                    story.OrganizationId == organizationId &&
                    story.IsExpired == false &&
                    story.ExpiresAt > utcNow,
                cancellationToken
            );
    }

    public async Task<int> ExpireStoriesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var utcNow = DateTime.UtcNow;

        return await _context.Stories
            .Where(story =>
                story.IsExpired == false &&
                story.ExpiresAt <= utcNow
            )
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(story => story.IsExpired, true)
                    .SetProperty(story => story.UpdatedAt, utcNow),
                cancellationToken
            );
    }
}
