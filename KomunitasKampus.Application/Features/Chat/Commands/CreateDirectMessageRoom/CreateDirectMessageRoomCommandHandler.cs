using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Enums;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateDirectMessageRoom;

public class CreateDirectMessageRoomCommandHandler : IRequestHandler<CreateDirectMessageRoomCommand, ChatRoomDto>
{
    private readonly IChatRoomRepository _chatRoomRepository;

    public CreateDirectMessageRoomCommandHandler(IChatRoomRepository chatRoomRepository)
    {
        _chatRoomRepository = chatRoomRepository;
    }

    public async Task<ChatRoomDto> Handle(CreateDirectMessageRoomCommand request, CancellationToken cancellationToken)
    {
        var existingRoom = await _chatRoomRepository.GetExistingDirectRoomAsync(request.InitiatorAccountId, request.TargetAccountId, cancellationToken);
        if (existingRoom is not null) return existingRoom.ToChatRoomDto();

        var now = DateTime.UtcNow;
        var room = new ChatRoom
        {
            OrganizationId = null,
            Name = null,
            IsMainGroup = false,
            RoomType = ChatRoomType.Direct,
            IsInviteOnly = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdRoom = await _chatRoomRepository.CreateAsync(room, cancellationToken);
        var participants = new[] { request.InitiatorAccountId, request.TargetAccountId };

        foreach (var accountId in participants)
        {
            await _chatRoomRepository.AddParticipantAsync(new ChatParticipant
            {
                RoomId = createdRoom.Id,
                AccountId = accountId,
                JoinedAt = now
            }, cancellationToken);
        }

        return createdRoom.ToChatRoomDto();
    }
}
