using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Entities;

namespace KomunitasKampus.Application.Features.Interactions;

internal static class InteractionAccessRules
{
    public static void EnsureAccountExists(Account? account)
    {
        if (account is null || account.DeletedAt is not null)
        {
            throw new NotFoundAppException("Akun tidak ditemukan.");
        }
    }

    public static void EnsurePostExists(Post? post)
    {
        if (post is null || post.DeletedAt is not null)
        {
            throw new NotFoundAppException("Postingan tidak ditemukan.");
        }
    }

    public static void EnsureNotOwnOrganizationPost(Account account, Post post, string actionName)
    {
        if (!IsOrganizationAccount(account))
        {
            return;
        }

        var isOwnPost =
            account.Organization?.Id == post.OrganizationId ||
            post.Organization?.AccountId == account.Id;

        if (!isOwnPost)
        {
            return;
        }

        throw new ForbiddenAccessAppException(
            $"Admin organisasi tidak boleh {actionName} postingan miliknya sendiri."
        );
    }

    public static bool IsInternalPost(Post post)
    {
        return string.Equals(
            post.Visibility.ToString(),
            "Internal",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static bool IsOrganizationAccount(Account account)
    {
        var role = account.Role.ToString();

        return string.Equals(role, "organization", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "organisasi", StringComparison.OrdinalIgnoreCase);
    }
}
