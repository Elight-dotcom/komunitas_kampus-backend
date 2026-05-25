using KomunitasKampus.Application.Common.Interfaces;

namespace KomunitasKampus.Infrastructure.Storage;

public class StoryUploadService : IStoryUploadService
{
    private readonly IMinioService _minioService;

    public StoryUploadService(IMinioService minioService)
    {
        _minioService = minioService;
    }

    public Task<string> GeneratePresignedPutUrlAsync(
        string fileKey,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        return _minioService.GeneratePresignedPutUrlAsync(
            fileKey,
            contentType,
            cancellationToken
        );
    }
}
