using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Stories.Commands.GeneratePresignedUploadUrl;

public sealed record StoryPresignedUploadUrlDto(
    string UploadUrl,
    string FileKey,
    StoryMediaType MediaType,
    long FileSize,
    DateTime ExpiresAt
);
