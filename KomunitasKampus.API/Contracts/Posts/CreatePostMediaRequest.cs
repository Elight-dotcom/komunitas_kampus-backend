using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.API.Contracts.Posts;

public sealed record CreatePostMediaRequest(
    string FileKey,
    PostMediaType MediaType,
    int FileSize,
    int OrderIndex
);
