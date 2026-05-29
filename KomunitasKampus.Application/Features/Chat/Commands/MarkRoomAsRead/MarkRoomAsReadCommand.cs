using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.MarkRoomAsRead;

public sealed record MarkRoomAsReadCommand(Guid RoomId, Guid AccountId) : IRequest<Unit>;
