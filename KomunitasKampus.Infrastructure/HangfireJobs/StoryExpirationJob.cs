using KomunitasKampus.Domain.Interfaces;
using KomunitasKampus.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace KomunitasKampus.Infrastructure.HangfireJobs;

public class StoryExpirationJob
{
    private readonly IStoryRepository _storyRepository;
    private readonly RedisStoryCacheService _storyCache;
    private readonly ILogger<StoryExpirationJob> _logger;

    public StoryExpirationJob(
        IStoryRepository storyRepository,
        RedisStoryCacheService storyCache,
        ILogger<StoryExpirationJob> logger
    )
    {
        _storyRepository = storyRepository;
        _storyCache = storyCache;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var expiredCount = await _storyRepository.ExpireStoriesAsync();

        if (expiredCount > 0)
        {
            await _storyCache.EvictActiveStoriesAsync();

            _logger.LogInformation(
                "Story expiration job selesai. {ExpiredCount} story di-expire dan cache stories:active:* dibersihkan.",
                expiredCount
            );

            return;
        }

        _logger.LogDebug(
            "Story expiration job selesai. Tidak ada story yang expired."
        );
    }
}
