namespace KomunitasKampus.Application.Features.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    Guid RecipientId,
    Guid? ActorId,
    string? ActorName,
    string? ActorAvatarUrl,
    string Type,
    Guid? ReferenceId,
    bool IsRead,
    DateTime CreatedAt
);