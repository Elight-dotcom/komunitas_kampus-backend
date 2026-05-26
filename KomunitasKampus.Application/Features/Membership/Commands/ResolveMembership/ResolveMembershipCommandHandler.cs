using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Application.Common.Interfaces;
using KomunitasKampus.Application.Features.Membership.DTOs;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Membership.Commands.ResolveMembership;

public class ResolveMembershipCommandHandler
    : IRequestHandler<ResolveMembershipCommand, MembershipDto>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IChatService _chatService;
    private readonly INotificationService _notificationService;

    public ResolveMembershipCommandHandler(
        IMembershipRepository membershipRepository,
        IChatService chatService,
        INotificationService notificationService
    )
    {
        _membershipRepository = membershipRepository;
        _chatService = chatService;
        _notificationService = notificationService;
    }

    public async Task<MembershipDto> Handle(
        ResolveMembershipCommand request,
        CancellationToken cancellationToken
    )
    {
        var membership = await _membershipRepository.GetByIdAsync(
            request.MembershipId,
            cancellationToken
        );

        if (membership is null)
        {
            throw new NotFoundAppException("Membership tidak ditemukan.");
        }

        if (membership.InviteType != MembershipInviteType.Request)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Command ini hanya untuk memproses join request mahasiswa."
                    }
                }
            );
        }

        if (membership.Status != MembershipStatus.Pending)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["membership"] = new[]
                    {
                        "Membership ini sudah pernah diproses."
                    }
                }
            );
        }

        if (membership.Organization?.AccountId != request.ResolvedBy)
        {
            throw new ForbiddenAccessAppException(
                "Hanya admin organisasi pemilik membership yang boleh memproses request ini."
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
            membership.Id,
            MembershipAccessRules.GetNotificationTypeForJoinRequest(nextStatus),
            cancellationToken
        );

        return membership.ToMembershipDto();
    }
}
