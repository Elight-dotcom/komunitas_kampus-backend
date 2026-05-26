using System.Text.Json;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace KomunitasKampus.Infrastructure.Caching;

public class RedisStoryCacheService
{
    private static readonly TimeSpan ActiveStoriesTtl = TimeSpan.FromMinutes(2);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisStoryCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public RedisStoryCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisStoryCacheService> logger
    )
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Story>?> GetActiveStoriesAsync(
        Guid? accountId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _redis.GetDatabase();
            var value = await database.StringGetAsync(GetActiveStoriesKey(accountId));

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            var cachedStories = JsonSerializer.Deserialize<List<CachedStory>>(
                value.ToString(),
                _jsonOptions
            );

            return cachedStories?
                .Select(ToEntity)
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Gagal membaca cache active stories untuk account {AccountId}. Database akan dipakai.",
                accountId
            );

            return null;
        }
    }

    public async Task SetActiveStoriesAsync(
        Guid? accountId,
        IReadOnlyList<Story> stories,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _redis.GetDatabase();

            var payload = JsonSerializer.Serialize(
                stories.Select(ToCachedStory).ToList(),
                _jsonOptions
            );

            await database.StringSetAsync(
                GetActiveStoriesKey(accountId),
                payload,
                ActiveStoriesTtl
            );
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Gagal menyimpan cache active stories untuk account {AccountId}.",
                accountId
            );
        }
    }

    public async Task EvictActiveStoriesAsync(
        Guid? accountId = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _redis.GetDatabase();

            if (accountId.HasValue)
            {
                await database.KeyDeleteAsync(GetActiveStoriesKey(accountId));
                return;
            }

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);

                if (!server.IsConnected)
                {
                    continue;
                }

                var keys = server
                    .Keys(pattern: "stories:active:*")
                    .ToArray();

                if (keys.Length > 0)
                {
                    await database.KeyDeleteAsync(keys);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Gagal menghapus cache active stories."
            );
        }
    }

    private static string GetActiveStoriesKey(Guid? accountId)
    {
        return accountId.HasValue
            ? $"stories:active:{accountId.Value:N}"
            : "stories:active:anonymous";
    }

    private static CachedStory ToCachedStory(Story story)
    {
        return new CachedStory(
            Id: story.Id,
            OrganizationId: story.OrganizationId,
            MediaType: story.MediaType.ToString(),
            MediaUrl: story.MediaUrl,
            TextContent: story.TextContent,
            IsExpired: story.IsExpired,
            ExpiresAt: story.ExpiresAt,
            CreatedAt: story.CreatedAt,
            UpdatedAt: story.UpdatedAt,
            DeletedAt: story.DeletedAt,
            Organization: story.Organization is null
                ? null
                : new CachedOrganization(
                    Id: story.Organization.Id,
                    AccountId: story.Organization.AccountId,
                    OrganizationName: story.Organization.OrganizationName,
                    Slug: story.Organization.Slug,
                    University: story.Organization.University,
                    AvatarUrl: story.Organization.Account?.AvatarUrl
                ),
            Views: story.Views
                .Select(view => new CachedStoryView(
                    Id: view.Id,
                    StoryId: view.StoryId,
                    AccountId: view.AccountId,
                    ViewedAt: view.ViewedAt,
                    CreatedAt: view.CreatedAt,
                    UpdatedAt: view.UpdatedAt,
                    DeletedAt: view.DeletedAt
                ))
                .ToList()
        );
    }

    private static Story ToEntity(CachedStory cached)
    {
        var mediaType = Enum.TryParse<StoryMediaType>(
            cached.MediaType,
            ignoreCase: true,
            out var parsedMediaType
        )
            ? parsedMediaType
            : StoryMediaType.Text;

        var story = new Story
        {
            Id = cached.Id,
            OrganizationId = cached.OrganizationId,
            MediaType = mediaType,
            MediaUrl = cached.MediaUrl,
            TextContent = cached.TextContent,
            IsExpired = cached.IsExpired,
            ExpiresAt = cached.ExpiresAt,
            CreatedAt = cached.CreatedAt,
            UpdatedAt = cached.UpdatedAt,
            DeletedAt = cached.DeletedAt,
            Organization = cached.Organization is null
                ? null
                : new Organization
                {
                    Id = cached.Organization.Id,
                    AccountId = cached.Organization.AccountId,
                    OrganizationName = cached.Organization.OrganizationName,
                    Slug = cached.Organization.Slug,
                    University = cached.Organization.University,
                    Account = new Account
                    {
                        Id = cached.Organization.AccountId,
                        AvatarUrl = cached.Organization.AvatarUrl
                    }
                }
        };

        story.Views = cached.Views
            .Select(view => new StoryView
            {
                Id = view.Id,
                StoryId = view.StoryId,
                AccountId = view.AccountId,
                ViewedAt = view.ViewedAt,
                CreatedAt = view.CreatedAt,
                UpdatedAt = view.UpdatedAt,
                DeletedAt = view.DeletedAt
            })
            .ToList();

        return story;
    }

    private sealed record CachedStory(
        Guid Id,
        Guid OrganizationId,
        string MediaType,
        string? MediaUrl,
        string? TextContent,
        bool IsExpired,
        DateTime ExpiresAt,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt,
        CachedOrganization? Organization,
        List<CachedStoryView> Views
    );

    private sealed record CachedOrganization(
        Guid Id,
        Guid AccountId,
        string OrganizationName,
        string Slug,
        string University,
        string? AvatarUrl
    );

    private sealed record CachedStoryView(
        Guid Id,
        Guid StoryId,
        Guid AccountId,
        DateTime ViewedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt
    );
}
