namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record SentInvitationDto(
    Guid MembershipId,
    Guid AccountId,
    Guid OrganizationId,
    string? Username,
    string? Email,
    string? FullName,
    string? University,
    string Status,
    DateTime RequestedAt,
    DateTime? ResolvedAt
);
