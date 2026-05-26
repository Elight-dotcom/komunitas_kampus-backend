using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Stories.DTOs;

public sealed record StoryDto(
    Guid Id,
    Guid OrganizationId,
    string OrgName,
    string? OrgAvatar,
    StoryMediaType MediaType,
    string? MediaUrl,
    string? TextContent,
    DateTime ExpiresAt,
    bool IsViewed
);
