using KomunitasKampus.Application.Features.Chat.DTOs;
using KomunitasKampus.Application.Common.Exceptions;
using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;

    public GetMessagesQueryHandler(IChatRoomRepository chatRoomRepository, IMessageRepository messageRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _messageRepository = messageRepository;
    }

    public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var isParticipant = await _chatRoomRepository.IsParticipantAsync(request.RoomId, request.RequestingAccountId, cancellationToken);
        if (!isParticipant) throw new ForbiddenAccessAppException("You are not a participant of this room");

        var messages = await _messageRepository.GetMessagesByRoomIdAsync(request.RoomId, request.Page, request.PageSize + 1, cancellationToken);
        var hasNextPage = messages.Count > request.PageSize;
        var items = messages.Take(request.PageSize).Select(message => message.ToMessageDto(request.RequestingAccountId)).ToList();

        return new PagedResult<MessageDto>(items, request.Page, request.PageSize, hasNextPage);
    }
}
