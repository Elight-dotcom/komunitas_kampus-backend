namespace KomunitasKampus.Application.Features.Interactions.DTOs;

public sealed record CommentDto(
    Guid Id,
    Guid UserId,
    string? Username,
    string? AvatarUrl,
    string? Role,
    string? Content,
    bool IsDeleted,
    string? DeletedReason,
    DateTime CreatedAt
);
