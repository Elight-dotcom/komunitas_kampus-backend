namespace KomunitasKampus.Infrastructure.Storage;

public interface IMinioService
{
    Task<string> GeneratePresignedPutUrlAsync(
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    );

    Task DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default
    );

    Task<string> GetFileUrlAsync(
        string fileKey,
        CancellationToken cancellationToken = default
    );
}
