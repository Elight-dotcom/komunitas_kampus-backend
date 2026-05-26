namespace KomunitasKampus.Application.Features.Organizations.DTOs;

public record OrganizationCardDto(
    Guid Id,
    string OrganizationName,
    string Slug,
    string University,
    string? Description,
    string? AvatarUrl,
    int MemberCount,
    int PostCount
);
