using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Unit>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;

    public DeleteMessageCommandHandler(IChatRoomRepository chatRoomRepository, IMessageRepository messageRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Unit> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);
        if (message is null) throw new NotFoundAppException("Message tidak ditemukan.");

        var isOwnMessage = message.SenderId == request.RequestingAccountId;
        var room = await _chatRoomRepository.GetByIdAsync(message.RoomId, cancellationToken);
        var isOrganizationOwnerOfRoom = room?.Organization?.AccountId == request.RequestingAccountId;
        if (!isOwnMessage && !isOrganizationOwnerOfRoom) throw new ForbiddenAccessAppException("You are not authorized to delete this message.");

        message.DeletedAt = DateTime.UtcNow;
        message.DeletedById = request.RequestingAccountId;
        message.UpdatedAt = DateTime.UtcNow;
        await _messageRepository.UpdateAsync(message, cancellationToken);
        return Unit.Value;
    }
}
