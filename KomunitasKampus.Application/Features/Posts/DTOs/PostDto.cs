namespace KomunitasKampus.Application.Features.Posts.DTOs;

public sealed record PostDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string? OrganizationAvatarUrl,
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
