namespace KomunitasKampus.Application.Features.Organizations.DTOs;

public record OrganizationDetailDto(
    Guid Id,
    string OrganizationName,
    string Slug,
    string University,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    int MemberCount,
    int PostCount,
    DateTime CreatedAt
);