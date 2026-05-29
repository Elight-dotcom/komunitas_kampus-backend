using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(Guid RoomId, Guid SenderId, string Content) : IRequest<MessageDto>;
