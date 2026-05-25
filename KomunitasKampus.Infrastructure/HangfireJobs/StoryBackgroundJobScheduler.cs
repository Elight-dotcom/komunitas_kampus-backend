using Hangfire;
using KomunitasKampus.Application.Common.Interfaces;

namespace KomunitasKampus.Infrastructure.HangfireJobs;

public class StoryBackgroundJobScheduler : IStoryBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public StoryBackgroundJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task EnqueueStoryVideoTranscodeAsync(
        Guid storyId,
        string fileKey,
        CancellationToken cancellationToken = default
    )
    {
        _backgroundJobClient.Enqueue<VideoTranscodeJob>(
            job => job.ProcessStoryVideoAsync(storyId, fileKey)
        );

        return Task.CompletedTask;
    }
}
