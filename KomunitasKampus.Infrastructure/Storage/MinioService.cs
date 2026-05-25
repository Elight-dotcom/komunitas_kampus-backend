using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace KomunitasKampus.Infrastructure.Storage;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;

    public MinioService(
        IMinioClient minioClient,
        IOptions<MinioSettings> options
    )
    {
        _minioClient = minioClient;
        _settings = options.Value;
    }

    public async Task<string> GeneratePresignedPutUrlAsync(
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File key wajib diisi.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type wajib diisi.", nameof(contentType));
        }

        var args = new PresignedPutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(NormalizeFileKey(fileName))
            .WithExpiry(_settings.PresignedUploadExpiryMinutes * 60);

        return await _minioClient.PresignedPutObjectAsync(args);
    }

    public async Task DeleteFileAsync(
        string fileKey,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            return;
        }

        var args = new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(NormalizeFileKey(fileKey));

        await _minioClient.RemoveObjectAsync(args, cancellationToken);
    }

    public Task<string> GetFileUrlAsync(
        string fileKey,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("File key wajib diisi.", nameof(fileKey));
        }

        var normalizedFileKey = NormalizeFileKey(fileKey);

        var baseUrl = string.IsNullOrWhiteSpace(_settings.PublicBaseUrl)
            ? BuildDefaultBaseUrl()
            : _settings.PublicBaseUrl.TrimEnd('/');

        var url = $"{baseUrl}/{_settings.BucketName}/{normalizedFileKey}";

        return Task.FromResult(url);
    }

    private string BuildDefaultBaseUrl()
    {
        var scheme = _settings.UseSsl ? "https" : "http";

        return $"{scheme}://{_settings.Endpoint.TrimEnd('/')}";
    }

    private static string NormalizeFileKey(string fileKey)
    {
        return fileKey.Replace("\\", "/").TrimStart('/');
    }
}
