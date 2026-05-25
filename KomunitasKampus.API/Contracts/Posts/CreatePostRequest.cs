using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.API.Contracts.Posts;

public sealed record CreatePostRequest(
    string Title,
    string? Caption,
    PostVisibility Visibility,
    bool IsPinned,
    int? PinOrder,
    IReadOnlyList<CreatePostMediaRequest> MediaItems
);
