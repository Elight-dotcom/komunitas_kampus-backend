namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record StudentSearchResultDto(
    Guid AccountId,
    string Username,
    string? FullName,
    string? University,
    string? AvatarUrl
);