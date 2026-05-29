using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateSubGroup;

public class CreateSubGroupCommandHandler : IRequestHandler<CreateSubGroupCommand, ChatRoomDto>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatAccountRepository _chatAccountRepository;

    public CreateSubGroupCommandHandler(IChatRoomRepository chatRoomRepository, IChatAccountRepository chatAccountRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatAccountRepository = chatAccountRepository;
    }

    public async Task<ChatRoomDto> Handle(CreateSubGroupCommand request, CancellationToken cancellationToken)
    {
        var organizationAccount = await _chatAccountRepository.GetByIdAsync(request.OrganizationAccountId, cancellationToken);
        if (organizationAccount is null) throw new NotFoundAppException("Akun organisasi tidak ditemukan.");
        if (organizationAccount.Role != AccountRole.Organisasi) throw new ForbiddenAccessAppException("Only organization account can create subgroup chat.");
        if (organizationAccount.Organization is null) throw new InvalidOperationException("Akun organization belum memiliki profil organisasi.");

        var now = DateTime.UtcNow;
        var room = new ChatRoom
        {
            OrganizationId = organizationAccount.Organization.Id,
            Name = request.Name.Trim(),
            IsMainGroup = false,
            RoomType = ChatRoomType.Sub,
            IsInviteOnly = request.IsInviteOnly,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdRoom = await _chatRoomRepository.CreateAsync(room, cancellationToken);
        var participantIds = request.MemberAccountIds.Append(request.OrganizationAccountId).Where(id => id != Guid.Empty).Distinct().ToList();
        foreach (var accountId in participantIds)
        {
            await _chatRoomRepository.AddParticipantAsync(new ChatParticipant { RoomId = createdRoom.Id, AccountId = accountId, JoinedAt = now }, cancellationToken);
        }

        createdRoom.Organization = organizationAccount.Organization;
        return createdRoom.ToChatRoomDto();
    }
}
