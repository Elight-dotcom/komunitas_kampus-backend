using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Posts;

internal static class PostAccessRules
{
    public static bool IsOrganizationRole(string role)
    {
        var normalizedRole = role.Trim().ToLowerInvariant();

        return normalizedRole is "organisasi" or "organization" or "admin_organisasi" or "adminorganization";
    }

    public static bool CanManageOrganization(
        string requesterRole,
        Guid? requesterOrganizationId,
        Guid targetOrganizationId
    )
    {
        return IsOrganizationRole(requesterRole)
            && requesterOrganizationId.HasValue
            && requesterOrganizationId.Value == targetOrganizationId;
    }

    public static bool CanViewPost(
        Post post,
        Guid? viewerAccountId,
        Guid? viewerOrganizationId,
        string? viewerRole,
        bool isAcceptedMember
    )
    {
        var isOwnerOrganization = viewerOrganizationId.HasValue
            && IsOrganizationRole(viewerRole ?? string.Empty)
            && viewerOrganizationId.Value == post.OrganizationId;

        if (isOwnerOrganization)
        {
            return true;
        }

        return post.Visibility switch
        {
            PostVisibility.Public => true,
            PostVisibility.Internal => viewerAccountId.HasValue && isAcceptedMember,
            PostVisibility.Private => false,
            _ => false
        };
    }
}
