using System.Diagnostics;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Infrastructure.Persistence;
using KomunitasKampus.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace KomunitasKampus.Infrastructure.HangfireJobs;

public class VideoTranscodeJob
{
    private readonly AppDbContext _context;
    private readonly IMinioClient _minioClient;
    private readonly IMinioService _minioService;
    private readonly MinioSettings _settings;
    private readonly ILogger<VideoTranscodeJob> _logger;

    public VideoTranscodeJob(
        AppDbContext context,
        IMinioClient minioClient,
        IMinioService minioService,
        IOptions<MinioSettings> options,
        ILogger<VideoTranscodeJob> logger
    )
    {
        _context = context;
        _minioClient = minioClient;
        _minioService = minioService;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task ProcessVideoAsync(
        Guid postMediaId,
        string fileKey
    )
    {
        var media = await _context.PostMedia
            .FirstOrDefaultAsync(item => item.Id == postMediaId);

        if (media is null)
        {
            _logger.LogWarning(
                "Video transcode skipped. PostMedia {PostMediaId} not found.",
                postMediaId
            );

            return;
        }

        if (media.MediaType != PostMediaType.Video)
        {
            _logger.LogWarning(
                "Video transcode skipped. PostMedia {PostMediaId} is not video.",
                postMediaId
            );

            return;
        }

        var outputKey = BuildProcessedPostVideoKey(postMediaId);

        await ProcessVideoFileAsync(
            sourceId: postMediaId,
            fileKey: fileKey,
            outputKey: outputKey,
            onCompletedAsync: async () =>
            {
                media.FileUrl = outputKey;
                media.Status = PostMediaStatus.Ready;
                media.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        );
    }

    public async Task ProcessStoryVideoAsync(
        Guid storyId,
        string fileKey
    )
    {
        var story = await _context.Stories
            .FirstOrDefaultAsync(item => item.Id == storyId);

        if (story is null)
        {
            _logger.LogWarning(
                "Story video transcode skipped. Story {StoryId} not found.",
                storyId
            );

            return;
        }

        if (story.MediaType != StoryMediaType.Video)
        {
            _logger.LogWarning(
                "Story video transcode skipped. Story {StoryId} is not video.",
                storyId
            );

            return;
        }

        var outputKey = BuildProcessedStoryVideoKey(storyId);

        await ProcessVideoFileAsync(
            sourceId: storyId,
            fileKey: fileKey,
            outputKey: outputKey,
            onCompletedAsync: async () =>
            {
                // Entity Story saat ini tidak memiliki kolom status.
                // Karena aturan "jangan ubah entity", proses transcode story hanya
                // mengganti MediaUrl dari file raw ke file processed.
                story.MediaUrl = outputKey;
                story.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        );
    }

    private async Task ProcessVideoFileAsync(
        Guid sourceId,
        string fileKey,
        string outputKey,
        Func<Task> onCompletedAsync
    )
    {
        var workDir = Path.Combine(
            Path.GetTempPath(),
            "komunitas-kampus-video",
            sourceId.ToString("N")
        );

        Directory.CreateDirectory(workDir);

        var inputPath = Path.Combine(workDir, "input");
        var outputPath = Path.Combine(workDir, "output.mp4");

        try
        {
            await DownloadFromMinioAsync(fileKey, inputPath);
            await RunFfmpegAsync(inputPath, outputPath);
            await UploadToMinioAsync(outputKey, outputPath);

            await onCompletedAsync();

            if (!string.Equals(fileKey, outputKey, StringComparison.OrdinalIgnoreCase))
            {
                await _minioService.DeleteFileAsync(fileKey);
            }
        }
        finally
        {
            TryDeleteDirectory(workDir);
        }
    }

    private async Task DownloadFromMinioAsync(
        string fileKey,
        string destinationPath
    )
    {
        var args = new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(NormalizeFileKey(fileKey))
            .WithFile(destinationPath);

        await _minioClient.GetObjectAsync(args);
    }

    private async Task UploadToMinioAsync(
        string fileKey,
        string filePath
    )
    {
        var args = new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(NormalizeFileKey(fileKey))
            .WithFileName(filePath)
            .WithContentType("video/mp4");

        await _minioClient.PutObjectAsync(args);
    }

    private async Task RunFfmpegAsync(
        string inputPath,
        string outputPath
    )
    {
        var arguments =
            $"-y -i \"{inputPath}\" -c:v libx264 -preset veryfast -crf 23 -c:a aac -movflags +faststart \"{outputPath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = _settings.FfmpegPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = processStartInfo
        };

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        if (process.ExitCode != 0)
        {
            _logger.LogError(
                "FFmpeg failed. Output: {Output}. Error: {Error}",
                standardOutput,
                standardError
            );

            throw new InvalidOperationException(
                $"FFmpeg gagal menjalankan transcode. Exit code: {process.ExitCode}"
            );
        }
    }

    private static string BuildProcessedPostVideoKey(Guid postMediaId)
    {
        return $"posts/videos/processed/{postMediaId:N}.mp4";
    }

    private static string BuildProcessedStoryVideoKey(Guid storyId)
    {
        return $"stories/videos/processed/{storyId:N}.mp4";
    }

    private static string NormalizeFileKey(string fileKey)
    {
        return fileKey.Replace("\\", "/").TrimStart('/');
    }

    private static void TryDeleteDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
        catch
        {
            // Temp cleanup tidak boleh membuat job dianggap gagal.
        }
    }
}
