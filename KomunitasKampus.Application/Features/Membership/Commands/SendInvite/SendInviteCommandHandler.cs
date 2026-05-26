using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendInvite;

public class SendInviteCommandHandler : IRequestHandler<SendInviteCommand, InviteDto>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationService _notificationService;

    public SendInviteCommandHandler(
        IMembershipRepository membershipRepository,
        IAccountRepository accountRepository,
        INotificationService notificationService
    )
    {
        _membershipRepository = membershipRepository;
        _accountRepository = accountRepository;
        _notificationService = notificationService;
    }

    public async Task<InviteDto> Handle(
        SendInviteCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!MembershipAccessRules.IsOrganizationRole(request.RequesterRole))
        {
            throw new ForbiddenAccessAppException(
                "Hanya akun organisasi yang boleh mengirim undangan anggota."
            );
        }

        if (request.RequesterOrganizationId != request.OrganizationId)
        {
            throw new ForbiddenAccessAppException(
                "Akun organisasi tidak boleh mengirim undangan atas nama organisasi lain."
            );
        }

        var targetAccount = await _accountRepository.GetByEmailOrUsernameAsync(
            request.TargetUsername.Trim(),
            cancellationToken
        );

        MembershipAccessRules.EnsureStudentAccount(targetAccount);

        var existingMembership = await _membershipRepository.GetByAccountAndOrgAsync(
            targetAccount!.Id,
            request.OrganizationId,
            cancellationToken
        );

        MembershipAccessRules.EnsureNoPendingOrAcceptedMembership(existingMembership);

        var isInCooldown = await _membershipRepository.IsInCooldownAsync(
            targetAccount.Id,
            request.OrganizationId,
            cancellationToken
        );

        if (isInCooldown)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Undangan baru baru bisa dikirim setelah cooldown 3 hari dari status rejected terakhir."
                    }
                }
            );
        }

        var membership = new Domain.Entities.Membership
        {
            AccountId = targetAccount.Id,
            OrganizationId = request.OrganizationId,
            Status = MembershipStatus.Pending,
            InviteType = MembershipInviteType.Invite,
            RequestedAt = DateTime.UtcNow
        };

        await _membershipRepository.CreateAsync(membership, cancellationToken);

        await _notificationService.SendMembershipNotificationAsync(
            targetAccount.Id,
            request.OrganizationId,
            membership.Id,
            "invite_sent",
            cancellationToken
        );

        return membership.ToInviteDto();
    }
}
