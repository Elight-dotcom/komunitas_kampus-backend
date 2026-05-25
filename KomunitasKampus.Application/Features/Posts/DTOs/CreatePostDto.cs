namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record CreatePostDto(
    string Title,
    string? Caption,
    bool IsPinned,
    int? PinOrder,
    string Visibility,
    IReadOnlyList<CreatePostMediaDto> Media
);
