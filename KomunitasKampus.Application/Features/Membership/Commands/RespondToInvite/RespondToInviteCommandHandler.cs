using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.RespondToInvite;

public class RespondToInviteCommandHandler
    : IRequestHandler<RespondToInviteCommand, InviteDto>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IChatService _chatService;
    private readonly INotificationService _notificationService;

    public RespondToInviteCommandHandler(
        IMembershipRepository membershipRepository,
        IChatService chatService,
        INotificationService notificationService
    )
    {
        _membershipRepository = membershipRepository;
        _chatService = chatService;
        _notificationService = notificationService;
    }

    public async Task<InviteDto> Handle(
        RespondToInviteCommand request,
        CancellationToken cancellationToken
    )
    {
        var membership = await _membershipRepository.GetByIdAsync(
            request.MembershipId,
            cancellationToken
        );

        if (membership is null)
        {
            throw new NotFoundAppException("Undangan tidak ditemukan.");
        }

        if (membership.InviteType != MembershipInviteType.Invite)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Membership ini bukan undangan organisasi."
                    }
                }
            );
        }

        if (membership.AccountId != request.AccountId)
        {
            throw new ForbiddenAccessAppException(
                "Undangan ini bukan milik akun yang sedang login."
            );
        }

        if (membership.Status != MembershipStatus.Pending)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Undangan ini sudah pernah direspons."
                    }
                }
            );
        }

        var nextStatus = MembershipAccessRules.ParseResolveAction(request.Action);
        var resolvedAt = DateTime.UtcNow;

        await _membershipRepository.UpdateStatusAsync(
            membership.Id,
            nextStatus,
            resolvedAt,
            cancellationToken
        );

        membership.Status = nextStatus;
        membership.ResolvedAt = resolvedAt;

        if (nextStatus == MembershipStatus.Accepted)
        {
            await _chatService.JoinMainGroupAsync(
                membership.OrganizationId,
                membership.AccountId,
                cancellationToken
            );
        }

        await _notificationService.SendMembershipNotificationAsync(
            membership.AccountId,
            membership.OrganizationId,
            MembershipAccessRules.GetNotificationTypeForInviteResponse(nextStatus),
            cancellationToken
        );

        return membership.ToInviteDto();
    }
}
