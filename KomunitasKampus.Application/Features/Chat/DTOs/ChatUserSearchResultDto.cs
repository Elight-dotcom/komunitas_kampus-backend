namespace KomunitasKampus.Application.Features.Chat.DTOs;

public sealed record ChatUserSearchResultDto(
    Guid AccountId,
    string Username,
    string? FullName,
    string? University,
    string? AvatarUrl
);