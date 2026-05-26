namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record PostDto(
    Guid Id,
    Guid OrganizationId,
    string Title,
    string? Caption,
    bool IsPinned,
    int? PinOrder,
    string Visibility,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PostMediaDto> Media
);
