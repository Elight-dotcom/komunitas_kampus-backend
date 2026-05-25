namespace KomunitasKampus.Application.Features.Stories.DTOs;

public sealed record StoryGroupDto(
    Guid OrganizationId,
    string OrgName,
    string? OrgAvatar,
    bool HasUnviewed,
    IReadOnlyList<StoryDto> Stories
);
