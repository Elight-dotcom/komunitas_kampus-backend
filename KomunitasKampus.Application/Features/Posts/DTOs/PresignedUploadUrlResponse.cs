using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record PresignedUploadUrlResponse(
    string UploadUrl,
    string FileKey,
    PostMediaType MediaType,
    int FileSize,
    DateTime ExpiresAt
);
