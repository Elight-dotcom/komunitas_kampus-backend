namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record CreatePostResponse(
    PostDto Post,
    IReadOnlyList<PresignedUploadDto> Uploads
);
