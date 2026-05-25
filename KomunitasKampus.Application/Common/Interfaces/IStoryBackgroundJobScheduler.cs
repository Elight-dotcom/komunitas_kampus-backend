namespace KomunitasKampus.Application.Common.Interfaces;

/// <summary>
/// Abstraction untuk menjadwalkan background job story.
/// Implementasi Infrastructure boleh memakai Hangfire dan reuse pipeline FFmpeg/VideoTranscodeJob yang sudah ada.
/// </summary>
public interface IStoryBackgroundJobScheduler
{
    Task EnqueueStoryVideoTranscodeAsync(
        Guid storyId,
        string fileKey,
        CancellationToken cancellationToken = default
    );
}
