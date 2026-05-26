using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;

namespace KomunitasKampus.Application.Features.Membership;

internal static class MembershipAccessRules
{
    public static bool IsOrganizationRole(string role)
    {
        return string.Equals(role, "organization", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "organisasi", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsStudentAccount(Account account)
    {
        var role = account.Role.ToString();

        var isStudentRole =
            string.Equals(role, "mahasiswa", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "student", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "user", StringComparison.OrdinalIgnoreCase);

        return isStudentRole && account.User is not null;
    }

    public static void EnsureStudentAccount(Account? account)
    {
        if (account is null || account.DeletedAt is not null)
        {
            throw new NotFoundAppException("Akun mahasiswa tidak ditemukan.");
        }

        if (!IsStudentAccount(account))
        {
            throw new ForbiddenAccessAppException(
                "Hanya akun mahasiswa dengan profil lengkap yang boleh menggunakan fitur membership."
            );
        }
    }

    public static void EnsureNoPendingOrAcceptedMembership(
        Domain.Entities.Membership? existingMembership
    )
    {
        if (existingMembership is null)
        {
            return;
        }

        if (existingMembership.Status == MembershipStatus.Pending)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Masih ada membership dengan status pending untuk organisasi ini."
                    }
                }
            );
        }

        if (existingMembership.Status == MembershipStatus.Accepted)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Akun ini sudah menjadi anggota organisasi."
                    }
                }
            );
        }
    }

    public static MembershipStatus ParseResolveAction(string action)
    {
        return action.Trim().ToLowerInvariant() switch
        {
            "accept" => MembershipStatus.Accepted,
            "accepted" => MembershipStatus.Accepted,
            "approve" => MembershipStatus.Accepted,
            "approved" => MembershipStatus.Accepted,
            "reject" => MembershipStatus.Rejected,
            "rejected" => MembershipStatus.Rejected,
            _ => throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["action"] = new[]
                    {
                        "Action hanya boleh accept atau reject."
                    }
                }
            )
        };
    }

    public static string GetNotificationTypeForJoinRequest(MembershipStatus status)
    {
        return status == MembershipStatus.Accepted
            ? "join_accepted"
            : "join_rejected";
    }

    public static string GetNotificationTypeForInviteResponse(MembershipStatus status)
    {
        return status == MembershipStatus.Accepted
            ? "invite_accepted"
            : "invite_rejected";
    }
}
