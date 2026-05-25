using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Features.Stories;

internal static class StoryAccessRules
{
    public static void EnsureStoryExists(Story? story)
    {
        if (story is null)
        {
            throw new NotFoundAppException("Story tidak ditemukan.");
        }
    }

    public static void EnsureStoryIsActive(Story story)
    {
        if (story.IsExpired || story.ExpiresAt <= DateTime.UtcNow)
        {
            throw new ForbiddenAccessAppException("Story sudah expired.");
        }
    }

    public static void EnsureOrganizationRole(string requesterRole)
    {
        var normalizedRole = requesterRole.Trim().ToLowerInvariant();

        if (normalizedRole is not ("organization" or "organisasi"))
        {
            throw new ForbiddenAccessAppException(
                "Hanya akun organisasi yang boleh membuat story."
            );
        }
    }

    public static void EnsureOrganizationOwnsStoryRequest(
        Guid organizationId,
        Guid? requesterOrganizationId
    )
    {
        if (!requesterOrganizationId.HasValue ||
            requesterOrganizationId.Value != organizationId)
        {
            throw new ForbiddenAccessAppException(
                "Akun organisasi hanya boleh membuat story untuk organisasinya sendiri."
            );
        }
    }
}
