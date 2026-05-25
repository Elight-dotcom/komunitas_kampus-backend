namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record PresignedUploadDto(
    string FileName,
    string MediaType,
    int OrderIndex,
    string UploadUrl
);
