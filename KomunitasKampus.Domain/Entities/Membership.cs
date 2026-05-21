using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Domain.Entities;

public class Membership : BaseEntity
{
    public Guid AccountId { get; set; }

    public Guid OrganizationId { get; set; }

    public MembershipStatus Status { get; set; }

    public MembershipInviteType InviteType { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public Account? Account { get; set; }

    public Organization? Organization { get; set; }
}