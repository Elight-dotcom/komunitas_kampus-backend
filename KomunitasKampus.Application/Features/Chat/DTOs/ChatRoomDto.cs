using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Chat.DTOs;

public sealed record ChatRoomDto(
    Guid RoomId,
    string? Name,
    ChatRoomType RoomType,
    bool IsMainGroup,
    bool IsInviteOnly,
    Guid? OrganizationId,
    DateTime CreatedAt
);

public sealed record ChatRoomSummaryDto(
    Guid RoomId,
    string? Name,
    ChatRoomType RoomType,
    bool IsMainGroup,
    string? LastMessageContent,
    DateTime? LastMessageAt,
    int UnreadCount,
    string? OtherParticipantAvatar
);

public sealed record MessageDto(
    Guid MessageId,
    Guid RoomId,
    Guid SenderId,
    string SenderUsername,
    string? SenderAvatarUrl,
    string? Content,
    bool IsDeleted,
    DateTime SentAt,
    bool IsOwnMessage
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    bool HasNextPage
);
