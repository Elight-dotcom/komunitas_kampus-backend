namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record MemberRequestDto(
    Guid MembershipId,
    Guid AccountId,
    Guid OrganizationId,
    string? Username,
    string? Email,
    string? FullName,
    string? University,
    string Status,
    string InviteType,
    DateTime RequestedAt
);
