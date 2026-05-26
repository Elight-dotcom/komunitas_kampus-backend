namespace KomunitasKampus.Infrastructure.Storage;

public class MinioSettings
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public string BucketName { get; init; } = string.Empty;

    public bool UseSsl { get; init; }

    public string? PublicBaseUrl { get; init; }

    public int PresignedUploadExpiryMinutes { get; init; } = 15;

    public string FfmpegPath { get; init; } = "ffmpeg";
}
