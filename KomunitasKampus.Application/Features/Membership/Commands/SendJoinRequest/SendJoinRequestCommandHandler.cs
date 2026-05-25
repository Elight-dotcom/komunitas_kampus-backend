using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.SendJoinRequest;

public class SendJoinRequestCommandHandler
    : IRequestHandler<SendJoinRequestCommand, MembershipDto>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IAccountRepository _accountRepository;

    public SendJoinRequestCommandHandler(
        IMembershipRepository membershipRepository,
        IAccountRepository accountRepository
    )
    {
        _membershipRepository = membershipRepository;
        _accountRepository = accountRepository;
    }

    public async Task<MembershipDto> Handle(
        SendJoinRequestCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await _accountRepository.GetByIdAsync(
            request.AccountId,
            cancellationToken
        );

        MembershipAccessRules.EnsureStudentAccount(account);

        var existingMembership = await _membershipRepository.GetByAccountAndOrgAsync(
            request.AccountId,
            request.OrganizationId,
            cancellationToken
        );

        MembershipAccessRules.EnsureNoPendingOrAcceptedMembership(existingMembership);

        var isInCooldown = await _membershipRepository.IsInCooldownAsync(
            request.AccountId,
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
                        "Kamu harus menunggu 3 hari setelah ditolak sebelum bisa mengirim join request lagi."
                    }
                }
            );
        }

        var membership = new Domain.Entities.Membership
        {
            AccountId = request.AccountId,
            OrganizationId = request.OrganizationId,
            Status = MembershipStatus.Pending,
            InviteType = MembershipInviteType.Request,
            RequestedAt = DateTime.UtcNow
        };

        await _membershipRepository.CreateAsync(membership, cancellationToken);

        return membership.ToMembershipDto();
    }
}
