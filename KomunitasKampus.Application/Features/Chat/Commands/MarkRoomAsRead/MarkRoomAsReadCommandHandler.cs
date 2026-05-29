using KomunitasKampus.Domain.Interfaces;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.MarkRoomAsRead;

public class MarkRoomAsReadCommandHandler : IRequestHandler<MarkRoomAsReadCommand, Unit>
{
    private readonly IChatReadStatusRepository _chatReadStatusRepository;

    public MarkRoomAsReadCommandHandler(IChatReadStatusRepository chatReadStatusRepository)
    {
        _chatReadStatusRepository = chatReadStatusRepository;
    }

    public async Task<Unit> Handle(MarkRoomAsReadCommand request, CancellationToken cancellationToken)
    {
        await _chatReadStatusRepository.UpsertReadStatusAsync(request.RoomId, request.AccountId, DateTime.UtcNow, cancellationToken);
        return Unit.Value;
    }
}
