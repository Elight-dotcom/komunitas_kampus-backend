using System.Text.Json;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace KomunitasKampus.Infrastructure.Caching;

public class RedisCommentCacheService
{
    private static readonly TimeSpan CommentCacheTtl = TimeSpan.FromMinutes(5);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCommentCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public RedisCommentCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCommentCacheService> logger
    )
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Comment>?> GetCommentsAsync(
        Guid postId,
        int page,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _redis.GetDatabase();
            var value = await database.StringGetAsync(GetCommentsKey(postId, page));

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            var cachedComments = JsonSerializer.Deserialize<List<CachedComment>>(
                value.ToString(),
                _jsonOptions
            );

            return cachedComments?
                .Select(ToEntity)
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Gagal membaca cache komentar untuk post {PostId}. Query database akan digunakan.",
                postId
            );

            return null;
        }
    }

    public async Task SetCommentsAsync(
        Guid postId,
        int page,
        IReadOnlyList<Comment> comments,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _redis.GetDatabase();

            var payload = JsonSerializer.Serialize(
                comments.Select(ToCachedComment).ToList(),
                _jsonOptions
            );

            await database.StringSetAsync(
                GetCommentsKey(postId, page),
                payload,
                CommentCacheTtl
            );
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Gagal menyimpan cache komentar untuk post {PostId}.",
                postId
            );
        }
    }

    public async Task EvictPostCommentsAsync(
        Guid postId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var pattern = $"comments:{postId}:*";
            var database = _redis.GetDatabase();

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);

                if (!server.IsConnected)
                {
                    continue;
                }

                var keys = server.Keys(pattern: pattern).ToArray();

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
                "Gagal menghapus cache komentar untuk post {PostId}.",
                postId
            );
        }
    }

    private static string GetCommentsKey(Guid postId, int page)
    {
        return $"comments:{postId}:{page}";
    }

    private static CachedComment ToCachedComment(Comment comment)
    {
        return new CachedComment(
            Id: comment.Id,
            UserId: comment.UserId,
            PostId: comment.PostId,
            Content: comment.Content,
            DeletedReason: comment.DeletedReason,
            DeletedById: comment.DeletedById,
            CreatedAt: comment.CreatedAt,
            UpdatedAt: comment.UpdatedAt,
            DeletedAt: comment.DeletedAt,
            User: comment.User is null
                ? null
                : new CachedAccount(
                    Id: comment.User.Id,
                    Username: comment.User.Username,
                    AvatarUrl: comment.User.AvatarUrl,
                    Role: comment.User.Role.ToString()
                ),
            DeletedBy: comment.DeletedBy is null
                ? null
                : new CachedAccount(
                    Id: comment.DeletedBy.Id,
                    Username: comment.DeletedBy.Username,
                    AvatarUrl: comment.DeletedBy.AvatarUrl,
                    Role: comment.DeletedBy.Role.ToString()
                )
        );
    }

    private static Comment ToEntity(CachedComment cached)
    {
        return new Comment
        {
            Id = cached.Id,
            UserId = cached.UserId,
            PostId = cached.PostId,
            Content = cached.Content,
            DeletedReason = cached.DeletedReason,
            DeletedById = cached.DeletedById,
            CreatedAt = cached.CreatedAt,
            UpdatedAt = cached.UpdatedAt,
            DeletedAt = cached.DeletedAt,
            User = cached.User is null ? null : ToAccount(cached.User),
            DeletedBy = cached.DeletedBy is null ? null : ToAccount(cached.DeletedBy)
        };
    }

    private static Account ToAccount(CachedAccount cached)
    {
        var role = Enum.TryParse<AccountRole>(
            cached.Role,
            ignoreCase: true,
            out var parsedRole
        )
            ? parsedRole
            : default;

        return new Account
        {
            Id = cached.Id,
            Username = cached.Username,
            AvatarUrl = cached.AvatarUrl,
            Role = role
        };
    }

    private sealed record CachedComment(
        Guid Id,
        Guid UserId,
        Guid PostId,
        string Content,
        string? DeletedReason,
        Guid? DeletedById,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? DeletedAt,
        CachedAccount? User,
        CachedAccount? DeletedBy
    );

    private sealed record CachedAccount(
        Guid Id,
        string Username,
        string? AvatarUrl,
        string Role
    );
}
