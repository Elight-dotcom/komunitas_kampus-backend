using KomunitasKampus.Application.Features.Membership.DTOs;

namespace KomunitasKampus.Application.Features.Membership;

public static class MembershipMappingExtensions
{
    public static MembershipDto ToMembershipDto(this Domain.Entities.Membership membership)
    {
        return new MembershipDto(
            Id: membership.Id,
            AccountId: membership.AccountId,
            OrganizationId: membership.OrganizationId,
            Username: membership.Account?.Username,
            Email: membership.Account?.Email,
            FullName: membership.Account?.User?.FullName,
            OrganizationName: membership.Organization?.OrganizationName,
            Status: membership.Status.ToString(),
            InviteType: membership.InviteType.ToString(),
            RequestedAt: membership.RequestedAt,
            ResolvedAt: membership.ResolvedAt
        );
    }

    public static MemberRequestDto ToMemberRequestDto(this Domain.Entities.Membership membership)
    {
        return new MemberRequestDto(
            MembershipId: membership.Id,
            AccountId: membership.AccountId,
            OrganizationId: membership.OrganizationId,
            Username: membership.Account?.Username,
            Email: membership.Account?.Email,
            FullName: membership.Account?.User?.FullName,
            University: membership.Account?.User?.University,
            Status: membership.Status.ToString(),
            InviteType: membership.InviteType.ToString(),
            RequestedAt: membership.RequestedAt
        );
    }

    public static InviteDto ToInviteDto(this Domain.Entities.Membership membership)
    {
        return new InviteDto(
            MembershipId: membership.Id,
            AccountId: membership.AccountId,
            OrganizationId: membership.OrganizationId,
            OrganizationName: membership.Organization?.OrganizationName,
            University: membership.Organization?.University,
            Status: membership.Status.ToString(),
            InviteType: membership.InviteType.ToString(),
            RequestedAt: membership.RequestedAt,
            ResolvedAt: membership.ResolvedAt
        );
    }
}
