namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record MembershipDto(
    Guid Id,
    Guid AccountId,
    Guid OrganizationId,
    string? Username,
    string? Email,
    string? FullName,
    string? OrganizationName,
    string Status,
    string InviteType,
    DateTime RequestedAt,
    DateTime? ResolvedAt
);
