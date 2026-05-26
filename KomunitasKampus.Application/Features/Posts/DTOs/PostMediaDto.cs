namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record PostMediaDto(
    Guid Id,
    string MediaType,
    string FileUrl,
    int FileSizeBytes,
    int OrderIndex,
    string Status
);
