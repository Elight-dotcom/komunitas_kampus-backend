namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record CreatePostMediaDto(
    string FileName,
    string MediaType,
    int FileSizeBytes,
    int OrderIndex
);
