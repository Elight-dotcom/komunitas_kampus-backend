using KomunitasKampus.Application.Features.Chat.DTOs;
using MediatR;

namespace KomunitasKampus.Application.Features.Chat.Commands.CreateDirectMessageRoom;

public sealed record CreateDirectMessageRoomCommand(Guid InitiatorAccountId, Guid TargetAccountId) : IRequest<ChatRoomDto>;
