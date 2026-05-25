using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.API.Contracts.Posts;

public sealed record PresignedUploadUrlRequest(
    string FileName,
    PostMediaType MediaType,
    int FileSize
);
