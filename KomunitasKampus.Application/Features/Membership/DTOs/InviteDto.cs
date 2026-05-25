namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record InviteDto(
    Guid MembershipId,
    Guid AccountId,
    Guid OrganizationId,
    string? OrganizationName,
    string? University,
    string Status,
    string InviteType,
    DateTime RequestedAt,
    DateTime? ResolvedAt
);
