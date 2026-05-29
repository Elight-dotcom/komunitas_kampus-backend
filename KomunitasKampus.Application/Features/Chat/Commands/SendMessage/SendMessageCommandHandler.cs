using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Entities;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;

    public SendMessageCommandHandler(IChatRoomRepository chatRoomRepository, IMessageRepository messageRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _messageRepository = messageRepository;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var isParticipant = await _chatRoomRepository.IsParticipantAsync(request.RoomId, request.SenderId, cancellationToken);
        if (!isParticipant) throw new ForbiddenAccessAppException("You are not a participant of this room");

        var message = new Message
        {
            RoomId = request.RoomId,
            SenderId = request.SenderId,
            Content = request.Content.Trim(),
            SentAt = DateTime.UtcNow
        };

        var createdMessage = await _messageRepository.CreateAsync(message, cancellationToken);
        return createdMessage.ToMessageDto(request.SenderId);
    }
}
