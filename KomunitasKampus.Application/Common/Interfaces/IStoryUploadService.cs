namespace KomunitasKampus.Application.Common.Interfaces;

public interface IStoryUploadService
{
    Task<string> GeneratePresignedPutUrlAsync(
        string fileKey,
        string contentType,
        CancellationToken cancellationToken = default
    );
}
