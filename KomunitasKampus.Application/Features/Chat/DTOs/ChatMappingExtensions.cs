using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Chat.DTOs;

public static class ChatMappingExtensions
{
    public static ChatRoomDto ToChatRoomDto(this ChatRoom room)
    {
        return new ChatRoomDto(
            RoomId: room.Id,
            Name: ResolveRoomName(room),
            RoomType: room.RoomType,
            IsMainGroup: room.IsMainGroup,
            IsInviteOnly: room.IsInviteOnly,
            OrganizationId: room.OrganizationId,
            CreatedAt: room.CreatedAt
        );
    }

    public static MessageDto ToMessageDto(this Message message, Guid requestingAccountId)
    {
        var isDeleted = message.DeletedAt is not null;

        return new MessageDto(
            MessageId: message.Id,
            RoomId: message.RoomId,
            SenderId: message.SenderId,
            SenderUsername: message.Sender?.Username ?? string.Empty,
            SenderAvatarUrl: message.Sender?.AvatarUrl,
            Content: isDeleted ? null : message.Content,
            IsDeleted: isDeleted,
            SentAt: message.SentAt,
            IsOwnMessage: message.SenderId == requestingAccountId
        );
    }

    public static ChatRoomSummaryDto ToChatRoomSummaryDto(this ChatRoom room, Guid accountId, int unreadCount)
    {
        var lastMessage = room.Messages
            .OrderByDescending(message => message.SentAt)
            .FirstOrDefault();

        var otherParticipantAvatar = room.RoomType == ChatRoomType.Direct
            ? room.Participants
                .Where(participant => participant.AccountId != accountId)
                .Select(participant => participant.Account?.AvatarUrl)
                .FirstOrDefault()
            : null;

        return new ChatRoomSummaryDto(
            RoomId: room.Id,
            Name: ResolveRoomName(room, accountId),
            RoomType: room.RoomType,
            IsMainGroup: room.IsMainGroup,
            LastMessageContent: lastMessage is null ? null : ResolveMessageSnippet(lastMessage),
            LastMessageAt: lastMessage?.SentAt,
            UnreadCount: unreadCount,
            OtherParticipantAvatar: otherParticipantAvatar
        );
    }

    private static string? ResolveRoomName(ChatRoom room, Guid? currentAccountId = null)
    {
        if (room.RoomType == ChatRoomType.Direct)
        {
            var otherParticipant = currentAccountId.HasValue
                ? room.Participants.FirstOrDefault(participant => participant.AccountId != currentAccountId.Value)
                : room.Participants.FirstOrDefault();

            return otherParticipant?.Account?.Username;
        }

        if (!string.IsNullOrWhiteSpace(room.Name)) return room.Name;

        if (room.IsMainGroup) return room.Organization?.OrganizationName;

        return "Chat Room";
    }

    private static string? ResolveMessageSnippet(Message message)
    {
        var isDeleted = message.DeletedAt is not null;

        if (isDeleted) return "Pesan telah dihapus";

        if (message.Content.Length <= 80) return message.Content;

        return $"{message.Content[..80]}...";
    }
}
