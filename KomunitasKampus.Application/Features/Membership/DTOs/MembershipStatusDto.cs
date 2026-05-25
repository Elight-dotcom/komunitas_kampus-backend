namespace KomunitasKampus.Application.Features.Membership.DTOs;

public sealed record MembershipStatusDto(
    bool HasMembership,
    string? Status,
    string? InviteType,
    bool IsInCooldown,
    DateTime? CanRequestAgainAt
);
